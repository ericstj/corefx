// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ------------------------------------------------------------------------------
// Changes to this file must follow the http://aka.ms/api-review process.
// ------------------------------------------------------------------------------

namespace System.Resources.Binary
{
    public partial class BinaryResourceWriter : System.Resources.IResourceWriter
    {
        public BinaryResourceWriter(System.IO.Stream stream) { }
        public BinaryResourceWriter(string fileName) { }
        protected virtual string ResourceReaderTypeName { get { throw null; } }
        protected virtual string ResourceSetTypeName { get { throw null; } }
        public System.Func<System.Type, string> TypeNameConverter { get { throw null; } set { } }
        public void AddResource(string name, byte[] value) { }
        public void AddResource(string name, System.IO.Stream value) { }
        public void AddResource(string name, System.IO.Stream value, bool closeAfterWrite) { }
        public void AddResource(string name, object value) { }
        public void AddResource(string name, string value) { }
        public void AddResourceData(string name, string typeName, byte[] serializedData) { }
        protected void AddResourceData(string name, string typeName, object dataContext) { }
        public void Close() { }
        public void Dispose() { }
        public void Generate() { }
        protected virtual void WriteData(System.IO.BinaryWriter writer, object dataContext) { }

        public void AddBinaryFormattedResource(string name, string typeName, byte[] value) { }
        public void AddStreamResource(string name, string typeName, byte[] value) { }
        public void AddStreamResource(string name, string typeName, System.IO.Stream value, bool closeAfterWrite) { }
        public void AddTypeConverterResource(string name, string typeName, byte[] value) { }
        public void AddTypeConverterResource(string name, string typeName, string value) { }
        
    }
}
