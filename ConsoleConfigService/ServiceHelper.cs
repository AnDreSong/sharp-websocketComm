using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

namespace ConsoleConfigService
{
    public class ServiceHelper
    {
        /// <summary>
        /// 服务是否存在
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool IsServiceExisted(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName == serviceName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="serviceName"></param>
        public static void StartService(string serviceName)
        {
            if (IsServiceExisted(serviceName))
            {
                System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Running &&
                    service.Status != System.ServiceProcess.ServiceControllerStatus.StartPending)
                {
                    service.Start();
                    for (int i = 0; i < 60; i++)
                    {
                        service.Refresh();
                        System.Threading.Thread.Sleep(1000);
                        if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                        {
                            break;
                        }
                        if (i == 59)
                        {
                            throw new Exception("Start Service Error：" + serviceName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取服务状态
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static ServiceControllerStatus GetServiceStatus(string serviceName)
        {
            System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);
            return service.Status;
        }

        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="install"></param>
        public static void ConfigService(string serviceName, bool install)
        {
            TransactedInstaller ti = new TransactedInstaller();
            ti.Installers.Add(new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            });
            ti.Installers.Add(new ServiceInstaller
            {
                DisplayName = serviceName,
                ServiceName = serviceName,
                Description = "串口监听转发服务",
                StartType = ServiceStartMode.Automatic//运行方式
            });
            ti.Context = new InstallContext();
            ti.Context.Parameters["assemblypath"] = "\"" + Assembly.GetEntryAssembly().Location + "\" /serialportWebservice.exe";
            if (install)
            {
                ti.Install(new Hashtable());
            }
            else
            {
                ti.Uninstall(null);
            }
        }
    }
}