// API simplificada do Unity 6: assembly UnityEngine.TestRunner (pacote com.unity.test-framework).
// Só as assinaturas que o jogo usa, para checar a compilação fora do Unity.
#pragma warning disable 0067, 0649, 0660, 0661, 0626, 0824
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.TestTools
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class UnityTestAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class UnitySetUpAttribute : Attribute { }
}

