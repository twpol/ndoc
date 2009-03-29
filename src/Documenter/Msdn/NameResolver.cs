using System.Collections.Specialized;
using NDoc3.Core.Reflection;

namespace NDoc3.Documenter.Msdn
{
	public class NameResolver
	{
		// <param name="fileNames">A StringDictionary holding id to file name mappings.</param>
		// <param name="elemNames">A StringDictionary holding id to element name mappings</param>

		private StringDictionary fileNames = new StringDictionary();
		private StringDictionary elemNames = new StringDictionary();

		public string GetFilenameForMember(string assemblyName, string memberId)
		{
			return fileNames[memberId];
		}

		public string GetDisplayNameForMember(string assemblyName, string memberId)
		{
			return elemNames[memberId];
		}

		public void RegisterNamespace(string assemblyName, string namespaceName)
		{
			string namespaceId = "N:" + namespaceName;
			fileNames[namespaceId] = GenerateFilenameForNamespace(assemblyName, namespaceName);
			elemNames[namespaceId] = namespaceName;
		}

		//		public string GetFilenameForNamespace(string assemblyName, string namespaceName)
		//		{
		//			return GenerateFilenameForNamespace(assemblyName, namespaceName);
		//		}
		//		public string GetFilenameForType(string assemblyName, string typeId)
		//		{
		//			return fileNames[typeId];
		//		}
		//		public string GetFilenameForConstructor(string assemblyName, string constructorId)
		//		{
		//			return fileNames[constructorId];
		//		}
		//
		//		public string GetFilenameForProperty(string assemblyName, string propertyId)
		//		{
		//			return fileNames[propertyId];
		//		}
		//
		//		public string GetFilenameForField(string assemblyName, string fieldId)
		//		{
		//			return fileNames[fieldId];
		//		}
		//		public string GetFilenameForOperator(string assemblyName, string operatorId)
		//		{
		//			return fileNames[operatorId];
		//		}
		//
		//		public string GetFilenameForEvent(string assemblyName, string eventId)
		//		{
		//			return fileNames[eventId];
		//		}
		//
		//		public string GetFilenameForMethod(string assemblyName, string methodId)
		//		{
		//			return fileNames[methodId];
		//		}
		//

		public void RegisterType(string assemblyName, string typeId, string displayName)
		{
			fileNames[typeId] = GenerateFilenameForType(assemblyName, typeId);
			elemNames[typeId] = displayName;
		}

		public void RegisterConstructor(string assemblyName, string typeId, string id, MethodContract contract, string overload)
		{
			fileNames[id] = GenerateFilenameForConstructor(assemblyName, id, contract, overload);
			elemNames[id] = elemNames[typeId];
		}

		public void RegisterOperator(string assemblyName, string memberId, string memberName, string overload)
		{
			fileNames[memberId] = GenerateFilenameForOperator(assemblyName, memberId, overload);
			elemNames[memberId] = memberName;
		}

		public void RegisterMethod(string assemblyName, string memberId, string memberName, string overload)
		{
			fileNames[memberId] = GenerateFilenameForMethod(assemblyName, memberId, overload);
			elemNames[memberId] = memberName;
		}

		public void RegisterProperty(string assemblyName, string memberId, string memberName, string overload)
		{
			fileNames[memberId] = GenerateFilenameForProperty(assemblyName, memberId, overload);
			elemNames[memberId] = memberName;
		}

		public void RegisterField(string assemblyName, string typeId, string memberId, bool isEnum, string memberName)
		{
			if (isEnum)
				fileNames[memberId] = fileNames[typeId];
			else
				fileNames[memberId] = GenerateFilenameForField(assemblyName, memberId);
			elemNames[memberId] = memberName;
		}

		public void RegisterEvent(string assemblyName, string memberId, string memberName)
		{
			fileNames[memberId] = GenerateFilenameForEvent(assemblyName, memberId);
			elemNames[memberId] = memberName;
		}

		private string GenerateFilenameForNamespace(string assemblyName, string namespaceName)
		{
			string fileName = namespaceName + ".html";
			return fileName;
		}

		private string GenerateFilenameForType(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + ".html";
			return fileName;
		}

		private string GenerateFilenameForConstructor(string assemblyName, string constructorID, MethodContract contract, string overload)
		{
			int dotHash = constructorID.IndexOf(".#"); // constructors could be #ctor or #cctor
			string fileName = constructorID.Substring(2, dotHash - 2);

			if (contract == MethodContract.Static)
				fileName += "Static";

			fileName += "Constructor";

			if (overload != null) {
				fileName += overload;
			}

			fileName += ".html";

			return fileName;
		}

		private string GenerateFilenameForEvent(string assemblyName, string eventID)
		{
			string fileName = eventID.Substring(2) + ".html";
			fileName = fileName.Replace("#",".");
			return fileName;
		}

		private string GenerateFilenameForProperty(string assemblyName, string propertyID, string overload)
		{
			string fileName = propertyID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1) {
				fileName = fileName.Substring(0, leftParenIndex);
			}

			if (overload != null) {
				fileName += overload;
			}

			fileName += ".html";
			fileName = fileName.Replace("#", ".");

			return fileName;
		}

		private string GenerateFilenameForField(string assemblyName, string fieldID)
		{
			string fileName = fieldID.Substring(2) + ".html";
			fileName = fileName.Replace("#", ".");
			return fileName;
		}

		private string GenerateFilenameForOperator(string assemblyName, string operatorID, string overload)
		{
			string fileName = operatorID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1) {
				fileName = fileName.Substring(0, leftParenIndex);
			}

			if (overload != null) {
				fileName += "_overload_" + overload;
			}

			fileName += ".html";
			fileName = fileName.Replace("#", ".");

			return fileName;
		}

		private string GenerateFilenameForMethod(string assemblyName, string methodID, string overload)
		{
			string fileName = methodID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1) {
				fileName = fileName.Substring(0, leftParenIndex);
			}

			fileName = fileName.Replace("#", ".");

			if (overload != null) {
				fileName += "_overload_" + overload;
			}

			fileName += ".html";

			return fileName;
		}

		public string GetFilenameForFields(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Fields.html";
			return fileName;
		}

		public string GetFilenameForOperators(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Operators.html";
			return fileName;
		}

		public string GetFilenameForMethods(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Methods.html";
			return fileName;
		}

		public string GetFilenameForProperties(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Properties.html";
			return fileName;
		}

		public string GetFilenameForEvents(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Events.html";
			return fileName;
		}

		public string GetFilenameForTypeHierarchy(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Hierarchy.html";
			return fileName;
		}

		public string GetFilenameForTypeMembers(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Members.html";
			return fileName;
		}

		public string GetFilenameForConstructors(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Constructor.html";
			return fileName;
		}
	}
}
