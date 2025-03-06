using Leaf.Language.Expressions;
using Leaf.Language.TypedExpressions;
using Leaf.Language.Api.Models;
using Leaf.Language.Core.Constants;
using Leaf.Language.Core.Expressions.BaseExpressions;
using Leaf.Language.Core.Models;
using Leaf.Language.Core.Parser;
using Leaf.Language.Core.Statements.BaseStatements;
using Leaf.Language.Core.Statements.TypedStatements;
using Leaf.Language.Core.StaticAnalysis;
using Tokenizer.Core.Exceptions;
using Tokenizer.Core.Models;

namespace Leaf.Language.Api;


public class LanguageInformationResolver: TypeResolver
{
    private ProgramContext _programContext = new();
    public ProgramContext Resolve(string filePath)
    {
        var parser = new ProgramParser();
        _programContext = new ProgramContext();
        var result = parser.ParseFile(filePath, out var errors);
        _programContext.Tokens = parser.GetTokens().ToList();
        _programContext.ValidationErrors.AddRange(errors.Select(x => (x.Token, x.Message)));
        ResolveWithTryCatch(result);
        return _programContext;
    }

    public ProgramContext ResolveText(string text)
    {
        var parser = new ProgramParser();
        _programContext = new ProgramContext();
        var result = parser.ParseText(text, out var errors);
        _programContext.Tokens = parser.GetTokens().ToList();
        _programContext.ValidationErrors.AddRange(errors.Select(x => (x.Token, x.Message)));
        ResolveWithTryCatch(result);
        return _programContext;
    }

    private void ResolveWithTryCatch(ParsingResult parsingResult)
    {
        GatherSignatures(parsingResult);

        foreach (var typeDefinition in parsingResult.TypeDefinitions)
        {
            if (RunWithTryCatch(() => ResolveTypeDefinition(typeDefinition), out var resolved) && resolved != null)
                _programContext.UserDefinedTypes.Add(resolved);
        }
        foreach (var statement in parsingResult.ImportLibraryDefinitions)
        {
            if (RunWithTryCatch(() => (TypedImportLibraryDefinition)statement.Resolve(this), out var resolved))
                _programContext.ImportLibraryDefinitions.Add(resolved!);
        }
        foreach (var statement in parsingResult.ImportedFunctionDefinitions)
        {
            if (RunWithTryCatch(() => (TypedImportedFunctionDefinition)statement.Resolve(this), out var resolved))
                _programContext.ImportedFunctionDefinitions.Add(resolved!);
        }
        foreach (var statement in parsingResult.FunctionDefinitions)
        {
            if (RunWithTryCatch(() => (TypedFunctionDefinition)statement.Resolve(this), out var resolved))
                _programContext.FunctionDefinitions.Add(resolved!);
        }
        _programContext.GenericTypeDefinitions.AddRange(_genericTypeDefinitions.Values);
        _programContext.GenericFunctionDefinitions.AddRange(_genericFunctionDefinitions.Values);
        _programContext.FunctionDefinitions.AddRange(_lambdaFunctions);
        _programContext.FunctionDefinitions.Reverse();
        if (parsingResult.ProgramIconStatement != null && RunWithTryCatch(() => (TypedProgramIconStatement)parsingResult.ProgramIconStatement.Resolve(this), out var typedProgramIconStatement))
            _programContext.ProgamIcon = typedProgramIconStatement;
    }

    private void GatherSignatures(ParsingResult parsingResult)
    {
        _localVariableTypeMap = new();
        _functionDefinitions = new();
        _importedFunctionDefinitions = new();
        _importLibraries = new();
        _currentFunctionTarget = null;
        foreach (var statement in parsingResult.TypeDefinitions)
        {
            RunWithTryCatch(() => statement.GatherSignature(this));
        }
        foreach (var statement in parsingResult.GenericTypeDefinitions)
        {
            RunWithTryCatch(() => statement.GatherSignature(this));
        }
        foreach (var statement in parsingResult.GenericFunctionDefinitions)
        {
            RunWithTryCatch(() => statement.GatherSignature(this));
        }
        foreach (var statement in parsingResult.ImportLibraryDefinitions)
        {
            RunWithTryCatch(() => statement.GatherSignature(this));
        }
        foreach (var statement in parsingResult.ImportedFunctionDefinitions)
        {
            RunWithTryCatch(() => statement.GatherSignature(this));
        }
        foreach (var statement in parsingResult.FunctionDefinitions)
        {
            RunWithTryCatch(() => statement.GatherSignature(this));
        }
    }


    #region GatherSignature

    public override void GatherSignature(TypeDefinition typeDefinition)
    {
        var typeSymbol = new TypeSymbol(typeDefinition.TypeName, new());
        if (_resolvedTypes.ContainsKey(typeSymbol))
            AddValidationError(typeDefinition.TypeName, $"redefinition of named type {typeDefinition.TypeName.Lexeme}");
        ReclassifyToken(typeDefinition.TypeName, ReclassifiedTokenTypes.Type);
        _resolvedTypes[typeSymbol] = new StructTypeInfo(typeDefinition.TypeName, new());
    }

    public override void GatherSignature(GenericTypeDefinition genericTypeDefinition)
    {
        if (_genericTypeDefinitions.ContainsKey(genericTypeDefinition.TypeName.Lexeme))
            AddValidationError(genericTypeDefinition.TypeName, $"redefinition of named generic type {genericTypeDefinition.TypeName.Lexeme}");
        ReclassifyToken(genericTypeDefinition.TypeName, ReclassifiedTokenTypes.Type);
        _genericTypeDefinitions[genericTypeDefinition.TypeName.Lexeme] = genericTypeDefinition;
    }

    public override void GatherSignature(GenericFunctionDefinition genericFunctionDefinition)
    {
        if (_genericFunctionDefinitions.ContainsKey(genericFunctionDefinition.FunctionName.Lexeme))
            AddValidationError(genericFunctionDefinition.FunctionName, $"redefinition of named generic function {genericFunctionDefinition.FunctionName.Lexeme}");
        _genericFunctionDefinitions[genericFunctionDefinition.FunctionName.Lexeme] = genericFunctionDefinition;
    }

    public override void GatherSignature(FunctionDefinition functionDefinition)
    {
        if (functionDefinition.Parameters.DistinctBy(x => x.Name.Lexeme).Count() != functionDefinition.Parameters.Count)
            AddValidationError(functionDefinition.FunctionName, $"redefinition of parameter name");
        var resolvedParameters = functionDefinition.Parameters.Select(x => new TypedParameter(ReclassifyToken(x.Name, ReclassifiedTokenTypes.Parameter), Resolve(x.TypeSymbol))).ToList();

        var invalidParameter = resolvedParameters.Find(x => !x.TypeInfo.IsStackAllocatable);
        if (invalidParameter != null)
            AddValidationError(invalidParameter.Name, $"invalid parameter type {invalidParameter.TypeInfo}: type is not stack allocatable");

        var functionBody = new List<TypedExpression>();

        if (_functionDefinitions.ContainsKey(functionDefinition.FunctionName.Lexeme))
            AddValidationError(functionDefinition.FunctionName, $"redefinition of function {functionDefinition.FunctionName.Lexeme}");
        if (_importedFunctionDefinitions.ContainsKey(functionDefinition.FunctionName.Lexeme))
            AddValidationError(functionDefinition.FunctionName, $"redefinition of symbol {functionDefinition.FunctionName.Lexeme}");

        _functionDefinitions[functionDefinition.FunctionName.Lexeme] = new TypedFunctionDefinition(functionDefinition, ReclassifyToken(functionDefinition.FunctionName, ReclassifiedTokenTypes.Function), Resolve(functionDefinition.ReturnType), resolvedParameters, functionBody, functionDefinition.CallingConvention, functionDefinition.IsExported, functionDefinition.ExportedSymbol);
    }

    public override void GatherSignature(ImportedFunctionDefinition importedFunctionDefinition)
    {
        if (importedFunctionDefinition.Parameters.DistinctBy(x => x.Name.Lexeme).Count() != importedFunctionDefinition.Parameters.Count)
            AddValidationError(importedFunctionDefinition.FunctionName, $"redefinition of parameter name");
        var resolvedParameters = importedFunctionDefinition.Parameters.Select(x => new TypedParameter(ReclassifyToken(x.Name, ReclassifiedTokenTypes.Parameter), Resolve(x.TypeSymbol))).ToList();
        var returnType = Resolve(importedFunctionDefinition.ReturnType);
        if (!returnType.IsStackAllocatable && !returnType.Is(IntrinsicType.Void))
            AddValidationError(importedFunctionDefinition.FunctionName, $"invalid size of return. Type is {returnType}");
        var invalidParameter = resolvedParameters.Find(x => !x.TypeInfo.IsStackAllocatable);
        if (invalidParameter != null)
            AddValidationError(invalidParameter.Name, $"invalid parameter type {invalidParameter.TypeInfo}: type is not stack allocatable");

        if (_functionDefinitions.ContainsKey(importedFunctionDefinition.FunctionName.Lexeme))
            AddValidationError(importedFunctionDefinition.FunctionName, $"redefinition of function {importedFunctionDefinition.FunctionName.Lexeme}");
        if (_importedFunctionDefinitions.ContainsKey(importedFunctionDefinition.FunctionName.Lexeme))
            AddValidationError(importedFunctionDefinition.FunctionName, $"redefinition of imported symbol {importedFunctionDefinition.FunctionName.Lexeme}");

        if (!_importLibraries.ContainsKey(importedFunctionDefinition.LibraryAlias.Lexeme))
            AddValidationError(importedFunctionDefinition.LibraryAlias, $"unable to import function from undefined library '{importedFunctionDefinition.LibraryAlias.Lexeme}'");

        _importedFunctionDefinitions[importedFunctionDefinition.FunctionName.Lexeme] = new TypedImportedFunctionDefinition(importedFunctionDefinition, ReclassifyToken(importedFunctionDefinition.FunctionName, ReclassifiedTokenTypes.ImportedFunction), returnType, resolvedParameters, importedFunctionDefinition.CallingConvention, importedFunctionDefinition.LibraryAlias, importedFunctionDefinition.FunctionSymbol);
    }

    public override void GatherSignature(ImportLibraryDefinition importLibraryDefinition)
    {
        if (_importLibraries.ContainsKey(importLibraryDefinition.LibraryAlias.Lexeme))
            AddValidationError(importLibraryDefinition.LibraryAlias, $"import library with alias {importLibraryDefinition.LibraryAlias.Lexeme} is already defined");
        _importLibraries[importLibraryDefinition.LibraryAlias.Lexeme] = new TypedImportLibraryDefinition(importLibraryDefinition, importLibraryDefinition.LibraryAlias, importLibraryDefinition.LibraryPath);
    }


    #endregion

    #region Statements
    public override TypedStatement Resolve(ProgramIconStatement programIconStatement)
    {
        return new TypedProgramIconStatement(programIconStatement, programIconStatement.IconFilePath);
    }

    public override StructTypeInfo ResolveTypeDefinition(TypeDefinition typeDefinition)
    {
        var typeSymbol = new TypeSymbol(typeDefinition.TypeName, new());
        if (!_resolvedTypes.TryGetValue(typeSymbol, out var foundType))
        {
            AddValidationError(typeDefinition.TypeName, $"unable to find type signature {typeSymbol}");
            foundType = new StructTypeInfo(typeDefinition.TypeName, new());
        }

        foreach (var field in typeDefinition.Fields)
        {
            foundType.Fields.Add(new(Resolve(field.TypeSymbol), field.Name));
        }
        foundType.ValidateFields();
        return foundType;
    }


    public override TypedStatement Resolve(FunctionDefinition functionDefinition)
    {
        if (!_functionDefinitions.TryGetValue(functionDefinition.FunctionName.Lexeme, out _currentFunctionTarget))
            throw new ParsingException(functionDefinition.FunctionName, $"unresolved function reference {functionDefinition.FunctionName.Lexeme}");

        _localVariableTypeMap = new();
        foreach (var expression in functionDefinition.BodyStatements)
        {
            CurrentFunctionTarget.BodyStatements.Add(expression.Resolve(this));
        }
        var returnIndex = CurrentFunctionTarget.BodyStatements.FindIndex(x => x is TypedReturnExpression);
        if (returnIndex == -1) AddValidationError(functionDefinition.FunctionName, $"function {functionDefinition.FunctionName.Lexeme} does not return");
        if (returnIndex != CurrentFunctionTarget.BodyStatements.Count - 1)
        {
            var unreachableStatement = CurrentFunctionTarget.BodyStatements[returnIndex + 1];
            AddValidationError(unreachableStatement.OriginalExpression.Token, $"unreachable code detected");
        }
        return CurrentFunctionTarget;
    }

    public override TypedStatement Resolve(ImportedFunctionDefinition importedFunctionDefinition)
    {
        if (!_importedFunctionDefinitions.TryGetValue(importedFunctionDefinition.FunctionName.Lexeme, out var typedImportedFunctionDefinition))
            throw new ParsingException(importedFunctionDefinition.FunctionName, $"unresolved imported function reference {importedFunctionDefinition.FunctionName.Lexeme}");

        return typedImportedFunctionDefinition;
    }

    public override TypedStatement Resolve(ImportLibraryDefinition importLibraryDefinition)
    {
        if (!_importLibraries.TryGetValue(importLibraryDefinition.LibraryAlias.Lexeme, out var typedImportLibraryDefinition))
            throw new ParsingException(importLibraryDefinition.LibraryAlias, $"unresolved library reference {importLibraryDefinition.LibraryAlias.Lexeme}");

        return typedImportLibraryDefinition;
    }

    public override TypedStatement Resolve(GenericFunctionDefinition genericFunctionDefinition)
    {
        throw new InvalidOperationException("this will not be called, unless from a derived class");
    }

    public override TypedStatement Resolve(GenericTypeDefinition genericTypeDefinition)
    {
        throw new InvalidOperationException("this will not be called, unless from a derived class");
    }

    public override TypedStatement Resolve(TypeDefinition typeDefinition)
    {
        throw new InvalidOperationException("this will not be called, unless from a derived class");
    }

    #endregion

    #region Expressions

    public override TypedExpression Resolve(CallExpression callExpression)
    {
        var args = callExpression.Arguments.Select(x => x.Resolve(this)).ToList();
        ITypedFunctionInfo? directCallTarget = null;
        if (callExpression.CallTarget is IdentifierExpression identifierExpression)
            directCallTarget = ResolveCallTarget(identifierExpression, args);
        else if (callExpression.CallTarget is GenericFunctionReferenceExpression genericFunctionReferenceExpression)
            directCallTarget = ResolveCallTarget(genericFunctionReferenceExpression, args);
        if (directCallTarget != null)
        {
            if (args.Count == directCallTarget.Parameters.Count)
            {
                for (int i = 0; i < directCallTarget.Parameters.Count; i++)
                {
                    if (!directCallTarget.Parameters[i].TypeInfo.Equals(args[i].TypeInfo))
                        AddValidationError(args[i].OriginalExpression.Token, $"call {directCallTarget.FunctionName.Lexeme}: expected argument to be of type {directCallTarget.Parameters[i].TypeInfo} but got {args[i].TypeInfo}");
                }            
            }
            else AddValidationError(callExpression.Token, $"parity mismatch in call {directCallTarget.FunctionName.Lexeme}: expected {directCallTarget.Parameters.Count} arguments but got {args.Count}");

            return new TypedDirectCallExpression(directCallTarget.ReturnType, callExpression, callExpression.CallTarget.Token, directCallTarget, args);
        }

        TypedExpression callTarget = callExpression.CallTarget.Resolve(this);

        if (!callTarget.TypeInfo.IsFunctionPtr)
        {
            AddValidationError(callExpression.Token, $"expect call target to be of type fn[...,t] but got {callTarget.TypeInfo}");
            return new TypedCallExpression(TypeInfo.Void, callExpression, callTarget, args);
        }

        if (args.Count == callTarget.TypeInfo.FunctionParameterTypes.Count)
        {
            for (int i = 0; i < callTarget.TypeInfo.FunctionParameterTypes.Count; i++)
            {
                if (!callTarget.TypeInfo.FunctionParameterTypes[i].Equals(args[i].TypeInfo))
                    AddValidationError(callExpression.Token, $"call {callTarget.TypeInfo}:expected argument to be of type {callTarget.TypeInfo.FunctionParameterTypes[i]} but got {args[i].TypeInfo}");
            }
        } 
        else AddValidationError(callExpression.Token, $"parity mismatch in call {callTarget.TypeInfo}: expected {callTarget.TypeInfo.FunctionParameterTypes.Count} arguments but got {args.Count}");
        
        return new TypedCallExpression(callTarget.TypeInfo.FunctionReturnType, callExpression, callTarget, args);
    }

    public override TypedExpression Resolve(CompilerIntrinsic_GetExpression compilerIntrinsic_GetExpression)
    {
        var retrievedType = Resolve(compilerIntrinsic_GetExpression.RetrievedType);
        var contextPointer = compilerIntrinsic_GetExpression.ContextPointer.Resolve(this);
        if (!contextPointer.TypeInfo.IsValidNormalPtr) AddValidationError(compilerIntrinsic_GetExpression.Token, $"retrieval context expects pointer type but got {contextPointer.TypeInfo}");
        return new TypedCompilerIntrinsic_GetExpression(retrievedType, compilerIntrinsic_GetExpression, contextPointer, compilerIntrinsic_GetExpression.MemberOffset);
    }

    public override TypedExpression Resolve(CompilerIntrinsic_SetExpression compilerIntrinsic_SetExpression)
    {
        var valueToAssign = compilerIntrinsic_SetExpression.ValueToAssign.Resolve(this);
        var contextPointer = compilerIntrinsic_SetExpression.ContextPointer.Resolve(this);
        if (!contextPointer.TypeInfo.IsValidNormalPtr) AddValidationError(compilerIntrinsic_SetExpression.Token, $"memory context expects pointer type but got {contextPointer.TypeInfo}");
        return new TypedCompilerIntrinsic_SetExpression(TypeInfo.Void, compilerIntrinsic_SetExpression, contextPointer, compilerIntrinsic_SetExpression.AssignmentOffset, valueToAssign);
    }

    public override TypedExpression Resolve(GetExpression getExpression)
    {
        var instance = getExpression.Instance.Resolve(this);
        ReclassifyToken(getExpression.TargetField, ReclassifiedTokenTypes.TypeField);
        if (instance.TypeInfo.IsValidNormalPtr && instance.TypeInfo.GenericTypeArgument!.IsStructType)
        {
            try
            {
                var fieldType = instance.TypeInfo.GenericTypeArgument.GetFieldType(getExpression.TargetField);
                return new TypedGetExpression(fieldType, getExpression, instance, getExpression.TargetField, getExpression.ShortCircuitOnNull);
            }
            catch (ParsingException pe)
            {
                _programContext.AddValidationError(pe);
                return new TypedGetExpression(TypeInfo.Void, getExpression, instance, getExpression.TargetField, getExpression.ShortCircuitOnNull);
            }
            
        }
        AddValidationError(getExpression.Token, $"expect valid pointer type on left hand side of member accessor");
        return new TypedGetExpression(TypeInfo.Void, getExpression, instance, getExpression.TargetField, getExpression.ShortCircuitOnNull);
    }

    public override TypedExpression Resolve(IdentifierExpression identifierExpression)
    {
        var foundType = CurrentFunctionTarget.Parameters.Find(x => x.Name.Lexeme == identifierExpression.Token.Lexeme)?.TypeInfo;
        if (foundType != null)
        {
            ReclassifyToken(identifierExpression.Token, ReclassifiedTokenTypes.Parameter);
        }
        else if (_localVariableTypeMap.TryGetValue(identifierExpression.Token.Lexeme, out foundType))
        {
            ReclassifyToken(identifierExpression.Token, ReclassifiedTokenTypes.Variable);
        }
        else
        {
            if (_functionDefinitions.TryGetValue(identifierExpression.Token.Lexeme, out var functionWithMatchingName))
            {
                ReclassifyToken(identifierExpression.Token, ReclassifiedTokenTypes.Function);
                return new TypedFunctionPointerExpression(functionWithMatchingName.GetFunctionPointerType(), identifierExpression, functionWithMatchingName); // identifiers only reference the function address so they can be used as lambdas
            }
            if (_importedFunctionDefinitions.TryGetValue(identifierExpression.Token.Lexeme, out var importedFunctionWithMatchingName))
            {
                ReclassifyToken(identifierExpression.Token, ReclassifiedTokenTypes.ImportedFunction);
                return new TypedFunctionPointerExpression(importedFunctionWithMatchingName.GetFunctionPointerType(), identifierExpression, importedFunctionWithMatchingName);
            }
        }
        if (foundType == null)
        {
            AddValidationError(identifierExpression.Token, $"unresolved symbol {identifierExpression.Token.Lexeme}");
            foundType = TypeInfo.Void;
        }

        return new TypedIdentifierExpression(foundType, identifierExpression, identifierExpression.Token);
    }

    public override TypedExpression Resolve(InlineAssemblyExpression inlineAssemblyExpression)
    {
        return new TypedInlineAssemblyExpression(TypeInfo.Void, inlineAssemblyExpression, inlineAssemblyExpression.AssemblyInstruction);
    }

    public override TypedExpression Resolve(LiteralExpression literalExpression)
    {
        if (literalExpression.Value == null)
            return new TypedLiteralExpression(TypeInfo.Pointer(TypeInfo.Void), literalExpression, null);
        if (literalExpression.Value.GetType() == typeof(string))
            return new TypedLiteralExpression(TypeInfo.String, literalExpression, literalExpression.Value);
        if (literalExpression.Value.GetType() == typeof(int))
            return new TypedLiteralExpression(TypeInfo.Integer, literalExpression, literalExpression.Value);
        if (literalExpression.Value.GetType() == typeof(float))
            return new TypedLiteralExpression(TypeInfo.Float, literalExpression, literalExpression.Value);
        AddValidationError(literalExpression.Token, $"unsupported literal type {literalExpression.Value.GetType().Name}");
        return new TypedLiteralExpression(TypeInfo.Void, literalExpression, null);
    }

    public override TypedExpression Resolve(LocalVariableExpression localVariableExpression)
    {
        var typeInfo = Resolve(localVariableExpression.TypeSymbol);
        if (!typeInfo.IsStackAllocatable)
            AddValidationError(localVariableExpression.Token, $"unable to create local variable of type {typeInfo}");
        if (CurrentFunctionTarget.Parameters.Any(x => x.Name.Lexeme == localVariableExpression.Identifier.Lexeme))
            AddValidationError(localVariableExpression.Identifier, $"symbol {localVariableExpression.Identifier.Lexeme} is already defined as a parameter");
        if (_localVariableTypeMap.ContainsKey(localVariableExpression.Identifier.Lexeme))
            AddValidationError(localVariableExpression.Identifier, $"symbol {localVariableExpression.Identifier.Lexeme} is already defined");
        _localVariableTypeMap.Add(localVariableExpression.Identifier.Lexeme, typeInfo);
        var initializer = localVariableExpression.Initializer?.Resolve(this);
        if (initializer != null && !initializer.TypeInfo.Equals(typeInfo))
            AddValidationError(localVariableExpression.Identifier, $"expect initializer value of type {typeInfo} but got {initializer.TypeInfo}");
        ReclassifyToken(localVariableExpression.Identifier, ReclassifiedTokenTypes.Variable);
        return new TypedLocalVariableExpression(TypeInfo.Void, localVariableExpression, localVariableExpression.Identifier, typeInfo, initializer);
    }

    public override TypedExpression Resolve(ReturnExpression returnExpression)
    {
        var returnValue = returnExpression.ReturnValue?.Resolve(this);
        if (CurrentFunctionTarget.ReturnType.Is(IntrinsicType.Void))
        {
            if (returnValue != null) AddValidationError(returnExpression.Token, $"unable to return value for function with return type of {IntrinsicType.Void}");
            else return new TypedReturnExpression(TypeInfo.Void, returnExpression, null);
        }
        if (returnValue == null)
            AddValidationError(returnExpression.Token, $"expected return type to match function return type of {CurrentFunctionTarget.ReturnType} but got None");
        else if (!returnValue.TypeInfo.Equals(CurrentFunctionTarget.ReturnType))
            AddValidationError(returnExpression.Token, $"expected return type to match function return type of {CurrentFunctionTarget.ReturnType} but got {returnValue.TypeInfo}");
        return new TypedReturnExpression(TypeInfo.Void, returnExpression, returnValue);
    }

    public override TypedExpression Resolve(SetExpression setExpression)
    {
        TypedExpression setTarget;
        if (setExpression.AssignmentTarget is IdentifierExpression identifierExpression)
        {
            setTarget = ResolveSetTarget(identifierExpression);
        }
        else if (setExpression.AssignmentTarget is GetExpression getExpression)
        {
            setTarget = Resolve(getExpression);
        }
        else
        {
            AddValidationError(setExpression.Token, $"expect assignment target to be identifier or member accessor");
            setTarget = setExpression.AssignmentTarget.Resolve(this);
        }
        var valueToAssign = setExpression.ValueToAssign.Resolve(this);
        if (!valueToAssign.TypeInfo.IsStackAllocatable)
            AddValidationError(setExpression.ValueToAssign.Token, $"invalid value transfer (type {valueToAssign.TypeInfo})");
        if (!setTarget.TypeInfo.Equals(valueToAssign.TypeInfo))
            AddValidationError(setExpression.Token, $"type mismatch: expected assignment value to be of type {setTarget.TypeInfo} but got {valueToAssign.TypeInfo}");
        return new TypedSetExpression(setTarget.TypeInfo, setExpression, setTarget, valueToAssign);
    }
    public override TypedExpression Resolve(CastExpression castExpression)
    {
        // We will trust the programmer on casts and only disallow casting to struct types bigger than 4 bytes
        var resolvedExpression = castExpression.Expression.Resolve(this);
        var typeInfo = Resolve(castExpression.TypeSymbol);
        if (typeInfo.IsStackAllocatable || typeInfo.SizeInMemory() == 4 && resolvedExpression.TypeInfo.SizeInMemory() == 4)
        {
            resolvedExpression.TypeInfo = typeInfo;
            return resolvedExpression;
        }
        AddValidationError(castExpression.Token, $"unable to cast type {resolvedExpression.TypeInfo} to type {typeInfo}");
        resolvedExpression.TypeInfo = typeInfo;
        return resolvedExpression;
    }

    public override TypedExpression Resolve(GenericFunctionReferenceExpression genericFunctionReferenceExpression)
    {
        var symbol = $"{genericFunctionReferenceExpression.Identifier.Lexeme}!{string.Join('_', genericFunctionReferenceExpression.TypeArguments.Select(x => x.GetFlattenedName()))}";
        if (_resolvedFunctionDefinitions.TryGetValue(symbol, out var resolvedFunctionDefinition)) return new TypedIdentifierExpression(resolvedFunctionDefinition.GetFunctionPointerType(), genericFunctionReferenceExpression, resolvedFunctionDefinition.FunctionName);
        if (!_genericFunctionDefinitions.TryGetValue(genericFunctionReferenceExpression.Identifier.Lexeme, out var genericFunctionDefinition))
            throw new ParsingException(genericFunctionReferenceExpression.Identifier, $"unresolved symbol to generic function definition {genericFunctionReferenceExpression.Identifier.Lexeme}");
        var functionDefinition = genericFunctionDefinition.ToFunctionDefinition(genericFunctionReferenceExpression.TypeArguments);
        GatherSignature(functionDefinition);
        var previousFunctionTarget = CurrentFunctionTarget;
        var previousVariableMap = _localVariableTypeMap;
        var typedFunctionDefinition = (TypedFunctionDefinition)Resolve(functionDefinition);
        AddToResolvedGenericFunctions(typedFunctionDefinition);
        _currentFunctionTarget = previousFunctionTarget;
        _localVariableTypeMap = previousVariableMap;
        var identifierExpression = new IdentifierExpression(typedFunctionDefinition.FunctionName);
        identifierExpression.StartToken = genericFunctionReferenceExpression.StartToken;
        identifierExpression.EndToken = genericFunctionReferenceExpression.EndToken;
        return Resolve(identifierExpression);
    }

    public override TypedExpression Resolve(LambdaExpression lambdaExpression)
    {
        // For lambdas we will simply pull out the function definition and return a reference to the function as an (unique, generated) identifier
        var anonymousToken = GetAnonymousFunctionLabel(lambdaExpression.FunctionDefinition.Token);
        lambdaExpression.FunctionDefinition.FunctionName = anonymousToken;
        GatherSignature(lambdaExpression.FunctionDefinition);
        var previousFunctionTarget = CurrentFunctionTarget;
        var previousVariableMap = _localVariableTypeMap;
        var flattenedLambda = (TypedFunctionDefinition)Resolve(lambdaExpression.FunctionDefinition);
        _lambdaFunctions.Add(flattenedLambda);
        _currentFunctionTarget = previousFunctionTarget;
        _localVariableTypeMap = previousVariableMap;
        var identifierExpression = new IdentifierExpression(anonymousToken);
        identifierExpression.StartToken = anonymousToken;
        identifierExpression.EndToken = anonymousToken;
        return Resolve(identifierExpression);
    }

    #endregion


    #region Helpers

    protected override void AddToResolvedGenericFunctions(TypedFunctionDefinition typedFunctionDefinition)
    {
        if (_resolvedFunctionDefinitions.ContainsKey(typedFunctionDefinition.FunctionName.Lexeme))
            AddValidationError(typedFunctionDefinition.FunctionName, $"redefinition of instantiated generic function {typedFunctionDefinition.FunctionName.Lexeme}");
        _resolvedFunctionDefinitions[typedFunctionDefinition.FunctionName.Lexeme] = typedFunctionDefinition;
    }

    public override TypeInfo Resolve(TypeSymbol typeSymbol)
    {
        if (typeSymbol.IsGenericTypeSymbol)
            AddValidationError(typeSymbol.TypeName, $"unable to resolve generic type parameter {typeSymbol} to a concrete type");
        if (typeSymbol.TypeName.Type == TokenTypes.IntrinsicType)
        {
            if (Enum.TryParse<IntrinsicType>(typeSymbol.TypeName.Lexeme, true, out var intrinsicType))
                return ResolveIntrinsicType(intrinsicType, typeSymbol.TypeName, typeSymbol.TypeArguments);
            else AddValidationError(typeSymbol.TypeName, $"invalid intrinsic type {typeSymbol}");
        }
        else
        {
            ReclassifyToken(typeSymbol.TypeName, ReclassifiedTokenTypes.Type);
        }
        if (_resolvedTypes.TryGetValue(typeSymbol, out var typeInfo))
            return typeInfo;
        if (typeSymbol.TypeArguments.Any() && _genericTypeDefinitions.TryGetValue(typeSymbol.TypeName.Lexeme, out var genericTypeDefinition))
        {
            if (genericTypeDefinition.GenericTypeParameters.Count != typeSymbol.TypeArguments.Count)
                AddValidationError(typeSymbol.TypeName, $"expect {genericTypeDefinition.GenericTypeParameters.Count} type arguments but got {typeSymbol.TypeArguments.Count}");
            var concreteTypeDefinition = genericTypeDefinition.ToConcreteTypeDefinition(typeSymbol.TypeArguments);
            GatherSignature(concreteTypeDefinition);
            return ResolveTypeDefinition(concreteTypeDefinition);
        }
        AddValidationError(typeSymbol.TypeName, $"unable to resolve type symbol {typeSymbol}");
        return TypeInfo.Void;
    }

    public override TypeInfo ResolveIntrinsicType(IntrinsicType intrinsicType, Token typeName, List<TypeSymbol> typeArguments)
    {
        if (intrinsicType == IntrinsicType.Ptr)
        {
            if (typeArguments.Count != 1) AddValidationError(typeName, "expect exactly one type argument");
            return TypeInfo.Pointer(Resolve(typeArguments[0]));
        }
        else if (intrinsicType == IntrinsicType.Func
            || intrinsicType == IntrinsicType.CFunc)
        {
            if (!typeArguments.Any()) AddValidationError(typeName, "expect at least one type argument");
            return new FunctionPtrTypeInfo(intrinsicType, typeArguments.Select(x => Resolve(x)).ToList());
        }
        if (intrinsicType == IntrinsicType.Void)
        {
            if (typeArguments.Any()) AddValidationError(typeName, $"type {typeName.Lexeme} does not support any type arguments");
            return TypeInfo.Void;
        }
        if (intrinsicType == IntrinsicType.Int)
        {
            if (typeArguments.Any()) AddValidationError(typeName, $"type {typeName.Lexeme} does not support any type arguments");
            return TypeInfo.Integer;
        }
        if (intrinsicType == IntrinsicType.Float)
        {
            if (typeArguments.Any()) AddValidationError(typeName, $"type {typeName.Lexeme} does not support any type arguments");
            return TypeInfo.Float;
        }
        if (intrinsicType == IntrinsicType.String)
        {
            if (typeArguments.Any()) AddValidationError(typeName, $"type {typeName.Lexeme} does not support any type arguments");
            return TypeInfo.String;
        }
        AddValidationError(typeName, $"unsupported intrinsic type {intrinsicType}");
        return TypeInfo.Void;
    }

    protected override ITypedFunctionInfo? ResolveCallTarget(IdentifierExpression identifierExpression, List<TypedExpression> arguments)
    {
        // Identifiers that are direct call targets will be handled differently IE
        // (printf msg) 

        if (_functionDefinitions.TryGetValue(identifierExpression.Token.Lexeme, out var functionWithMatchingName))
        {
            ReclassifyToken(identifierExpression.Token, ReclassifiedTokenTypes.Function);
            return functionWithMatchingName;
        }
        if (_importedFunctionDefinitions.TryGetValue(identifierExpression.Token.Lexeme, out var importedFunctionWithMatchingName))
        {
            ReclassifyToken(identifierExpression.Token, ReclassifiedTokenTypes.ImportedFunction);
            return importedFunctionWithMatchingName;
        }
        if (!_genericFunctionDefinitions.ContainsKey(identifierExpression.Token.Lexeme)) return null;
        return ResolveCallTarget((GenericFunctionReferenceExpression)new GenericFunctionReferenceExpression(identifierExpression.Token, new()).CopyStartAndEndTokens(identifierExpression), arguments);
    }

    protected override ITypedFunctionInfo ResolveCallTarget(GenericFunctionReferenceExpression genericFunctionReferenceExpression, List<TypedExpression> arguments)
    {
        if (genericFunctionReferenceExpression.TypeArguments.Count == 0)
        {
            // we will try to infer the generic type arguments
            if (!_genericFunctionDefinitions.TryGetValue(genericFunctionReferenceExpression.Identifier.Lexeme, out var genericFunction))
                throw new ParsingException(genericFunctionReferenceExpression.Identifier, $"unresolved symbol to generic function definition {genericFunctionReferenceExpression.Identifier.Lexeme}");
            if (arguments.Count != genericFunction.Parameters.Count)
                throw new ParsingException(genericFunctionReferenceExpression.Token, $"expected {genericFunction.Parameters.Count} arguments in call {genericFunction.FunctionName} but got {arguments.Count}");

            // attempt to extract generic type parameters from the argument types
            var genericTypeParameters = genericFunction.GenericTypeParameters;
            var argumentTypes = arguments.Select(x => x.TypeInfo).ToList();
            var genericParametersTypeMap = new Dictionary<TypeSymbol, TypeInfo>();
            for (int i = 0; i < genericFunction.Parameters.Count; i++)
            {
                if (!argumentTypes[i].TryExtractGenericArgumentTypes(genericParametersTypeMap, genericFunction.Parameters[i].TypeSymbol))
                    AddValidationError(genericFunctionReferenceExpression.Token, $"unable to resolve generic type arguments for call {genericFunction.FunctionName}");
            }
            var resolvedGenericTypeArguments = new List<TypeSymbol>();
            var missingParameters = new List<TypeSymbol>();
            foreach (var genericTypeParameter in genericFunction.GenericTypeParameters)
            {
                if (!genericParametersTypeMap.TryGetValue(genericTypeParameter, out var resolvedTypeArgument))
                    missingParameters.Add(genericTypeParameter);
                else resolvedGenericTypeArguments.Add(resolvedTypeArgument.ToTypeSymbol());
            }
            if (missingParameters.Any())
                AddValidationError(genericFunctionReferenceExpression.Token, $"unable to resolve all parameters for call {genericFunction.FunctionName}. Missing parameters {string.Join(", ", missingParameters.Select(x => x.ToString()))}");
            var newGenericFunctionReference = new GenericFunctionReferenceExpression(genericFunctionReferenceExpression.Token, resolvedGenericTypeArguments);
            newGenericFunctionReference.StartToken = genericFunctionReferenceExpression.StartToken;
            newGenericFunctionReference.EndToken = genericFunctionReferenceExpression.EndToken;
            return ResolveCallTarget(newGenericFunctionReference);
        }
        return ResolveCallTarget(genericFunctionReferenceExpression);
    }

    protected override ITypedFunctionInfo ResolveCallTarget(GenericFunctionReferenceExpression genericFunctionReferenceExpression)
    {
        var symbol = $"{genericFunctionReferenceExpression.Identifier.Lexeme}!{string.Join('_', genericFunctionReferenceExpression.TypeArguments.Select(x => x.GetFlattenedName()))}";
        if (_resolvedFunctionDefinitions.TryGetValue(symbol, out var resolvedFunctionDefinition))
        {
            ReclassifyToken(genericFunctionReferenceExpression.Identifier, ReclassifiedTokenTypes.Function);
            return resolvedFunctionDefinition;
        }
        if (!_genericFunctionDefinitions.TryGetValue(genericFunctionReferenceExpression.Identifier.Lexeme, out var genericFunctionDefinition))
            throw new ParsingException(genericFunctionReferenceExpression.Identifier, $"unresolved symbol to generic function definition {genericFunctionReferenceExpression.Identifier.Lexeme}");
        var functionDefinition = genericFunctionDefinition.ToFunctionDefinition(genericFunctionReferenceExpression.TypeArguments);
        GatherSignature(functionDefinition);
        var previousFunctionTarget = CurrentFunctionTarget;
        var previousVariableMap = _localVariableTypeMap;
        var typedFunctionDefinition = (TypedFunctionDefinition)Resolve(functionDefinition);
        AddToResolvedGenericFunctions(typedFunctionDefinition);
        _currentFunctionTarget = previousFunctionTarget;
        _localVariableTypeMap = previousVariableMap;
        ReclassifyToken(genericFunctionReferenceExpression.Identifier, ReclassifiedTokenTypes.Function);
        return typedFunctionDefinition;
    }

    protected override TypedExpression ResolveSetTarget(IdentifierExpression identifierExpression)
    {
        var foundType = CurrentFunctionTarget.Parameters.Find(x => x.Name.Lexeme == identifierExpression.Token.Lexeme)?.TypeInfo;
        if (foundType != null)
        {
            ReclassifyToken(identifierExpression.Token, ReclassifiedTokenTypes.Parameter);
            return new TypedIdentifierExpression(foundType, identifierExpression, identifierExpression.Token);
        }
        if (!_localVariableTypeMap.TryGetValue(identifierExpression.Token.Lexeme, out foundType))
        {
            AddValidationError(identifierExpression.Token, $"unresolved symbol {identifierExpression.Token.Lexeme}");
            foundType = TypeInfo.Void;
        }
        ReclassifyToken(identifierExpression.Token, ReclassifiedTokenTypes.Variable);
        return new TypedIdentifierExpression(foundType, identifierExpression, identifierExpression.Token);
    }

    private void AddValidationError(Token token, string message)
    {
        _programContext.AddValidationError(new ParsingException(token, message));
    }

    private void RunWithTryCatch(Action action)
    {
        try
        {
            action();
        }
        catch (ParsingException pe)
        {
            _programContext.AddValidationError(pe);
        }
    }

    private bool RunWithTryCatch<Ty>(Func<Ty> func, out Ty? tyVal) where Ty : class
    {
        tyVal = default;
        try
        {
            tyVal = func();
            return true;
        }
        catch (ParsingException pe)
        {
            _programContext.AddValidationError(pe);
            return false;
        }
    }


    #endregion
}