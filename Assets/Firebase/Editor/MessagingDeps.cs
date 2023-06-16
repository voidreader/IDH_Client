// <copyright file="MessagingDeps.cs" company="Google Inc.">
// Copyright (C) 2016 Google Inc. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// This file is used to define dependencies, and pass them along to a system
/// which can resolve dependencies.
/// </summary>
[InitializeOnLoad]
public class FirebaseMessagingDeps : AssetPostprocessor
{
    /// <summary>
    /// This is the entry point for "InitializeOnLoad". It will register the
    /// dependencies with the dependency tracking system.
    /// </summary>
    static FirebaseMessagingDeps()
    {
        SetupDeps();
    }

    static void SetupDeps()
    {

    }
}

