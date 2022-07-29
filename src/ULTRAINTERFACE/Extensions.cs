using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;

public static class Extensions {
	public static void InvokeNextFrame(this MonoBehaviour mb, Action method) {
		mb.StartCoroutine(InvokeNextFrameCoro(method));
	}

	public static IEnumerator InvokeNextFrameCoro(Action method) {
		yield return null;
		method();
	}
}