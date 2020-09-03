using DotNetNuke.Common.Utilities;


            #region Sync Extensions Methods            
                                //get all assemblies 
                                IEnumerable<IToolbarItem> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                            where t != (typeof(IToolbarItem)) && (typeof(IToolbarItem).IsAssignableFrom(t))
                                                                            select Activator.CreateInstance(t) as IToolbarItem;


            #endregion