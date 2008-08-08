using System;

namespace Xsd2Code
{
	/// <summary>
	/// Interface for extension types that wish to participate in the 
	/// WXS code generation process.
	/// </summary>
	public interface ICodeExtension
	{
		void Process( System.CodeDom.CodeNamespace code, System.Xml.Schema.XmlSchema schema );
	}
}
