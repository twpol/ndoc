using System;
using System.IO;
using System.Reflection;

namespace NDoc3.Core.Reflection {
	internal interface IAssemblyLoader {
		IAssemblyInfo GetAssemblyInfo(FileInfo assemblyFile);
		void AddSearchDirectory(ReferencePath path);
	}

	internal interface IClrElementInfo {
		T[] GetCustomAttributes<T>(bool inherit) where T : Attribute;
		AssemblyName AssemblyName { get; }
	}

	internal interface IAssemblyInfo : IClrElementInfo {
		string FullName { get; }
		AssemblyName GetName();
		AssemblyName[] GetReferencedAssemblies();

		//TODO: those below need to be abstracted
		IModuleInfo[] GetModules();
		Type[] GetTypes();
	}

	internal interface IModuleInfo : IClrElementInfo {
		string ScopeName { get; }

		//TODO: those below need to be abstracted
		Type[] GetTypes();
	}

	internal class ReflectionAssemblyInfo : IAssemblyInfo {
		private readonly Assembly _assembly;

		public ReflectionAssemblyInfo(Assembly assembly) {
			_assembly = assembly;
		}

		public string FullName {
			get { return _assembly.FullName; }
		}

		public AssemblyName GetName() {
			return _assembly.GetName();
		}

		public AssemblyName AssemblyName {
			get { return GetName(); }
		}

		public AssemblyName[] GetReferencedAssemblies() {
			return _assembly.GetReferencedAssemblies();
		}

		/// <summary>
		/// Returns the list of attributes
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="inherit"></param>
		/// <returns></returns>
		public T[] GetCustomAttributes<T>(bool inherit)
			where T : Attribute {
			object[] atts = _assembly.GetCustomAttributes(typeof(T), inherit);
			return Array.ConvertAll(atts, att => (T)att);
		}

		public IModuleInfo[] GetModules() {
			Module[] modules = _assembly.GetModules();
			return Array.ConvertAll(modules, module => new ReflectionModuleInfo(module));
		}

		public Type[] GetTypes() {
			return _assembly.GetTypes();
		}
	}

	internal class ReflectionModuleInfo : IModuleInfo {
		private readonly Module _module;

		public ReflectionModuleInfo(Module module) {
			_module = module;
		}

		public AssemblyName AssemblyName {
			get { return _module.Assembly.GetName(); }
		}

		public T[] GetCustomAttributes<T>(bool inherit) where T : Attribute {
			object[] atts = _module.GetCustomAttributes(typeof(T), inherit);
			return Array.ConvertAll(atts, att => (T)att);
		}

		public string ScopeName {
			get { return _module.ScopeName; }
		}

		public Type[] GetTypes() {
			return _module.GetTypes();
		}
	}
}
