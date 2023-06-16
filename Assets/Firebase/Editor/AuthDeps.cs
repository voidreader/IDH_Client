// <copyright file="AuthDeps.cs" company="Google Inc.">
// Copyright (C) 2016 Google Inc. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;

/// <summary>
/// This file is used to define dependencies, and pass them along to a system
/// which can resolve dependencies.
/// </summary>
[InitializeOnLoad]
public class FirebaseAuthDeps : AssetPostprocessor
{
    /// <summary>
    /// This is the entry point for "InitializeOnLoad". It will register the
    /// dependencies with the dependency tracking system.
    /// </summary>
    static FirebaseAuthDeps()
    {
        SetupDeps();
    }

    static void SetupDeps()
	{
	}
}

