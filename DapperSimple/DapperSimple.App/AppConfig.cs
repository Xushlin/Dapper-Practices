using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DapperSimple.App
{
   
        public class ApplicationConfig
        {
            public static void RegisterApplication(ApplicationInfo applicationInfo, VersionCollection versions, Assembly entryPointAssembly)
            {
                var assemblyCompanyAttribute = (AssemblyCompanyAttribute)entryPointAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true).FirstOrDefault();
                if (assemblyCompanyAttribute != null)
                {
                    applicationInfo.CompanyName = assemblyCompanyAttribute.Company;
                }
                var assemblyProductAttribute = (AssemblyProductAttribute)entryPointAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true).FirstOrDefault();
                if (assemblyProductAttribute != null)
                {
                    applicationInfo.ApplicationName = assemblyProductAttribute.Product;
                }

                var fileVersionInfo = FileVersionInfo.GetVersionInfo(entryPointAssembly.Location);
                var productVersion = fileVersionInfo.ProductVersion;
                var version = entryPointAssembly.GetName().Version;
                var assemblyVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Revision, version.Build);

                versions.PrimaryVersion = new VersionInfo()
                {
                    AssemblyVersion = assemblyVersion,
                    ProductVersion = productVersion
                };
            }
        }

        public class ApplicationInfo
        {
            static ApplicationInfo()
            {
                Instance = new ApplicationInfo();
            }
            public static ApplicationInfo Instance { get; private set; }
            public string ApplicationName { get; set; }
            public string CompanyName { get; set; }
        }

        public class VersionCollection : Collection<VersionInfo>
        {
            public VersionInfo PrimaryVersion { get; set; }
        }

        public static class VersionTable
        {
            static VersionTable()
            {
                Versions = new VersionCollection();
            }

            public static VersionCollection Versions { get; private set; }
        }

        public class VersionInfo
        {
            public string AssemblyVersion { get; set; }
            public string ProductVersion { get; set; }
        }
    }

