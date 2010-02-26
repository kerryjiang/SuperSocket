//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.IO;
//using System.Reflection;
//using GiantSoft.Common;
//using GiantSoft.SocketServiceCore.Config;
//using System.Net;

//namespace GiantSoft.SocketServiceCore
//{
//    /// <summary>
//    /// The base class of all service provider factory
//    /// This class load service provider dynamically
//    /// </summary>
//    public abstract class ServiceProviderFactory
//    {
//        private Dictionary<string, ProviderBase> m_ProviderDict = new Dictionary<string, ProviderBase>();

//        public abstract IRunable CreateServer(IPEndPoint endPoint);

//        /// <summary>
//        /// Setups the specified factory.
//        /// </summary>
//        /// <param name="assembly">The assembly name.</param>
//        /// <param name="config">The config.</param>
//        /// <returns></returns>
//        public bool Setup(string assembly, IServerConfig config)
//        {
//            if(string.IsNullOrEmpty(assembly))
//                return true;
				
//            string dir = FileHelper.GetParentFolder(this.GetType().Assembly.Location);
			
//            string assemblyFile	= Path.Combine(dir, assembly + ".dll");
			
//            try
//            {
//                Type typeProvider = typeof(ProviderBase);

//                Assembly ass = Assembly.LoadFrom(assemblyFile);

//                Type[] arrType = ass.GetTypes();
			
//                for(int i = 0; arrType!=null && i < arrType.Length; i++)
//                {
//                    //Must be a seal class
//                    if (arrType[i].IsSubclassOf(typeProvider) && arrType[i].IsSealed)
//                    {
//                        ProviderBase provider = ass.CreateInstance(arrType[i].ToString()) as ProviderBase;
//                        if(provider.Init(config))
//                        {
//                            m_ProviderDict[provider.Name.ToLower()] = provider;
//                        }
//                        else
//                        {
//                            LogUtil.LogError("Failed to initalize provider " + arrType[i].ToString() + "!");
//                            return false;
//                        }
//                    }
//                }
				
//                return true;
//            }
//            catch(Exception e)
//            {
//                LogUtil.LogError(e);
//                return false;
//            }
//        }

//        /// <summary>
//        /// Gets service provider by name.
//        /// </summary>
//        /// <param name="providerName">Name of the provider.</param>
//        /// <returns></returns>
//        public ProviderBase GetProviderByName(string providerName)
//        {
//            ProviderBase provider = null;
			
//            if(m_ProviderDict.TryGetValue(providerName.ToLower(), out provider))
//            {
//                return provider;
//            }
//            else
//            {
//                return null;
//            }			
//        }

//        /// <summary>
//        /// Gets a value indicating whether this <see cref="ServiceProviderFactory"/> is loaded.
//        /// </summary>
//        /// <value><c>true</c> if loaded; otherwise, <c>false</c>.</value>
//        public abstract bool Loaded { get; }
//    }
//}
