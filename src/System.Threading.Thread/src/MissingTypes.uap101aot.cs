// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Temporary type definitions to quiet GenFacades

namespace System.Threading
{
    public delegate void ParameterizedThreadStart(object obj);
    public sealed class Thread {}
    public class ThreadInterruptedException {}
    public delegate void ThreadStart();
    public sealed partial class ThreadStartException {}
    public enum ThreadState {}
    public class ThreadStateException {}
}