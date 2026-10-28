// Mini NUnit: só o pedaço do NUnit 3 que os testes de Assets/Tests/EditMode/Logica
// usam, com as mesmas assinaturas. Serve para rodar os testes das regras sem o
// Unity e sem o .NET SDK, só com o compilador C# que vem no Windows
// (veja rodar_testes.ps1). No Unity os testes usam o NUnit de verdade.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestFixtureAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SetUpAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TearDownAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestCaseAttribute : Attribute
    {
        public object[] Argumentos;

        public TestCaseAttribute(params object[] argumentos)
        {
            Argumentos = argumentos;
        }
    }

    public class AssertionException : Exception
    {
        public AssertionException(string mensagem) : base(mensagem)
        {
        }
    }

    public static class Assert
    {
        static string Msg(string message, object[] args)
        {
            if (string.IsNullOrEmpty(message))
                return "";
            return " - " + (args != null && args.Length > 0 ? string.Format(message, args) : message);
        }

        static bool Numero(object o)
        {
            return o is int || o is long || o is float || o is double || o is short || o is byte || o is decimal;
        }

        static bool Iguais(object a, object b)
        {
            if (a == null || b == null)
                return a == null && b == null;
            if (Numero(a) && Numero(b))
                return Convert.ToDouble(a) == Convert.ToDouble(b);
            return a.Equals(b);
        }

        public static void AreEqual(object expected, object actual)
        {
            AreEqual(expected, actual, null);
        }

        public static void AreEqual(object expected, object actual, string message, params object[] args)
        {
            if (!Iguais(expected, actual))
                throw new AssertionException("Esperado <" + expected + "> mas foi <" + actual + ">" + Msg(message, args));
        }

        public static void AreEqual(double expected, double actual, double delta)
        {
            AreEqual(expected, actual, delta, null);
        }

        public static void AreEqual(double expected, double actual, double delta, string message, params object[] args)
        {
            if (Math.Abs(expected - actual) > delta)
                throw new AssertionException("Esperado " + expected + " +/- " + delta + " mas foi " + actual + Msg(message, args));
        }

        public static void AreNotEqual(object expected, object actual)
        {
            AreNotEqual(expected, actual, null);
        }

        public static void AreNotEqual(object expected, object actual, string message, params object[] args)
        {
            if (Iguais(expected, actual))
                throw new AssertionException("Não devia ser <" + actual + ">" + Msg(message, args));
        }

        public static void IsTrue(bool condition)
        {
            IsTrue(condition, null);
        }

        public static void IsTrue(bool condition, string message, params object[] args)
        {
            if (!condition)
                throw new AssertionException("Esperado verdadeiro" + Msg(message, args));
        }

        public static void IsFalse(bool condition)
        {
            IsFalse(condition, null);
        }

        public static void IsFalse(bool condition, string message, params object[] args)
        {
            if (condition)
                throw new AssertionException("Esperado falso" + Msg(message, args));
        }

        public static void IsNull(object anObject)
        {
            IsNull(anObject, null);
        }

        public static void IsNull(object anObject, string message, params object[] args)
        {
            if (anObject != null)
                throw new AssertionException("Esperado null mas foi <" + anObject + ">" + Msg(message, args));
        }

        public static void IsNotNull(object anObject)
        {
            IsNotNull(anObject, null);
        }

        public static void IsNotNull(object anObject, string message, params object[] args)
        {
            if (anObject == null)
                throw new AssertionException("Não esperado null" + Msg(message, args));
        }

        public static void Greater(double arg1, double arg2)
        {
            Greater(arg1, arg2, null);
        }

        public static void Greater(double arg1, double arg2, string message, params object[] args)
        {
            if (!(arg1 > arg2))
                throw new AssertionException("Esperado " + arg1 + " > " + arg2 + Msg(message, args));
        }

        public static void GreaterOrEqual(double arg1, double arg2)
        {
            GreaterOrEqual(arg1, arg2, null);
        }

        public static void GreaterOrEqual(double arg1, double arg2, string message, params object[] args)
        {
            if (!(arg1 >= arg2))
                throw new AssertionException("Esperado " + arg1 + " >= " + arg2 + Msg(message, args));
        }

        public static void Less(double arg1, double arg2)
        {
            Less(arg1, arg2, null);
        }

        public static void Less(double arg1, double arg2, string message, params object[] args)
        {
            if (!(arg1 < arg2))
                throw new AssertionException("Esperado " + arg1 + " < " + arg2 + Msg(message, args));
        }

        public static void LessOrEqual(double arg1, double arg2)
        {
            LessOrEqual(arg1, arg2, null);
        }

        public static void LessOrEqual(double arg1, double arg2, string message, params object[] args)
        {
            if (!(arg1 <= arg2))
                throw new AssertionException("Esperado " + arg1 + " <= " + arg2 + Msg(message, args));
        }

        public static void Fail(string message)
        {
            throw new AssertionException(message);
        }
    }

    public static class CollectionAssert
    {
        public static void IsEmpty(IEnumerable collection, string message, params object[] args)
        {
            var itens = new List<string>();
            foreach (object o in collection)
                itens.Add(o == null ? "null" : o.ToString());
            if (itens.Count > 0)
                throw new AssertionException("Esperado vazio, mas tem " + itens.Count + ": " + string.Join(" | ", itens.ToArray()) +
                                             (string.IsNullOrEmpty(message) ? "" : " - " + message));
        }

        public static void Contains(IEnumerable collection, object actual)
        {
            foreach (object o in collection)
            {
                if (Equals(o, actual))
                    return;
            }
            throw new AssertionException("A coleção não contém <" + actual + ">");
        }
    }
}

namespace MiniNUnit
{
    using NUnit.Framework;

    /// <summary>Procura [Test] e [TestCase] nas classes carregadas e roda tudo.</summary>
    public static class Executor
    {
        public static int Main(string[] args)
        {
            int ok = 0;
            var falhas = new List<string>();
            var tipos = new List<Type>();
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.FullName.StartsWith("mscorlib") || a.FullName.StartsWith("System"))
                    continue;
                try
                {
                    tipos.AddRange(a.GetTypes());
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }
            tipos.Sort(delegate(Type x, Type y) { return string.CompareOrdinal(x.FullName, y.FullName); });
            foreach (Type t in tipos)
            {
                if (t.GetCustomAttributes(typeof(TestFixtureAttribute), true).Length == 0)
                    continue;
                MethodInfo setup = null;
                MethodInfo teardown = null;
                foreach (MethodInfo m in t.GetMethods())
                {
                    if (m.GetCustomAttributes(typeof(SetUpAttribute), true).Length > 0)
                        setup = m;
                    if (m.GetCustomAttributes(typeof(TearDownAttribute), true).Length > 0)
                        teardown = m;
                }
                foreach (MethodInfo m in t.GetMethods())
                {
                    var casos = new List<object[]>();
                    if (m.GetCustomAttributes(typeof(TestAttribute), true).Length > 0)
                        casos.Add(new object[0]);
                    foreach (TestCaseAttribute tc in m.GetCustomAttributes(typeof(TestCaseAttribute), true))
                        casos.Add(tc.Argumentos);
                    foreach (object[] caso in casos)
                    {
                        string nome = t.Name + "." + m.Name + (caso.Length > 0 ? "(" + string.Join(", ", Array.ConvertAll(caso, Convert.ToString)) + ")" : "");
                        try
                        {
                            object instancia = Activator.CreateInstance(t);
                            if (setup != null)
                                setup.Invoke(instancia, null);
                            try
                            {
                                m.Invoke(instancia, Converter(m, caso));
                            }
                            finally
                            {
                                if (teardown != null)
                                    teardown.Invoke(instancia, null);
                            }
                            ok++;
                        }
                        catch (TargetInvocationException e)
                        {
                            falhas.Add(nome + "\n      " + e.InnerException.GetType().Name + ": " + e.InnerException.Message);
                        }
                        catch (Exception e)
                        {
                            falhas.Add(nome + "\n      " + e.GetType().Name + ": " + e.Message);
                        }
                    }
                }
            }
            foreach (string f in falhas)
                Console.WriteLine("FALHOU  " + f);
            Console.WriteLine();
            Console.WriteLine("Testes: " + (ok + falhas.Count) + "   passaram: " + ok + "   falharam: " + falhas.Count);
            return falhas.Count == 0 ? 0 : 1;
        }

        static object[] Converter(MethodInfo m, object[] caso)
        {
            ParameterInfo[] p = m.GetParameters();
            var r = new object[p.Length];
            for (int i = 0; i < p.Length; i++)
                r[i] = Convert.ChangeType(caso[i], p[i].ParameterType);
            return r;
        }
    }
}
