using System.Reflection;
using System.Reflection.Emit;

namespace TeamAwake.Core.Network.Extensions;

internal static class ReflectionExtensions
{
	/// <summary>Creates a custom delegate from <paramref name="method" />.</summary>
	/// <param name="method">The method to extract delegate.</param>
	/// <typeparam name="TFirst">The first parameter.</typeparam>
	/// <typeparam name="TSecond">The second parameter.</typeparam>
	/// <typeparam name="TResult">The third parameter.</typeparam>
	/// <returns>A custom delegate.</returns>
	/// <exception cref="InvalidCastException">When a parameter is not cast.</exception>
	public static Func<object, TFirst, TSecond, TResult> CreateDelegate<TFirst, TSecond, TResult>(this MethodInfo method)
    {
        var returnType = typeof(TResult);

        var methodParameters = method.GetParameters()
            .Select(p => p.ParameterType)
            .ToArray();

        var delegateParameters = new[] { typeof(TFirst), typeof(TSecond) };

        var finalParameters = new[] { typeof(object) }
            .Concat(delegateParameters)
            .ToArray();

        var dynamicMethod = new DynamicMethod(string.Empty, returnType, finalParameters, true);
        var generator = dynamicMethod.GetILGenerator();

        if (!method.IsStatic)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(method.DeclaringType!.IsClass ? OpCodes.Castclass : OpCodes.Unbox, method.DeclaringType);
        }

        for (var i = 0; i < delegateParameters.Length; i++)
        {
            generator.Emit(OpCodes.Ldarg, i + 1);

            var methodParameter = methodParameters[i];
            var delegateParameter = delegateParameters[i];

            if (delegateParameter == methodParameter)
                continue;

            if (!methodParameter.IsSubclassOf(delegateParameter) && methodParameter.IsAssignableTo(delegateParameter))
                throw new InvalidCastException($"Cannot cast {delegateParameter.Name} to {methodParameter.Name}");

            generator.Emit(methodParameter.IsClass ? OpCodes.Castclass : OpCodes.Unbox, methodParameter);
        }

        generator.Emit(OpCodes.Call, method);

        if (returnType != method.ReturnType)
        {
            if (!method.ReturnType.IsSubclassOf(returnType) && !method.ReturnType.IsAssignableTo(returnType))
                throw new InvalidCastException($"Cannot cast {method.ReturnType.Name} to {returnType.Name}");

            if (method.ReturnType.IsClass && returnType.IsClass)
                generator.Emit(OpCodes.Castclass, returnType);

            else if (returnType == typeof(object))
                generator.Emit(OpCodes.Box, method.ReturnType);

            else if (method.ReturnType.IsClass)
                generator.Emit(OpCodes.Unbox, returnType);
        }

        generator.Emit(OpCodes.Ret);

        return dynamicMethod.CreateDelegate<Func<object, TFirst, TSecond, TResult>>();
    }
}