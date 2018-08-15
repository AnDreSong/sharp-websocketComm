using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.ServiceProcess;
using System.Text;
using serialportWebservice;

namespace ConsoleConfigService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ///带参启动运行服务
            if (args.Length > 0)
            {
                try
                {
                    ServiceBase[] serviceToRun = new ServiceBase[] { new PortService(), };
                    ServiceBase.Run(serviceToRun);
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(@"D:\Log.txt", "\nService Start Error：" + DateTime.Now.ToString() + "\n" + ex.Message);
                }
            }
            //不带参启动配置程序
            else
            {
                StartLable:
                Console.WriteLine("\n\n请选择你要执行的操作——1：自动部署服务，2：安装服务，3：卸载服务，4：验证服务状态，5：退出");
                Console.WriteLine("————————————————————");
                var key = Console.ReadLine();

                if (key == 1.ToString())
                {
                    if (ServiceHelper.IsServiceExisted("PortService"))
                    {
                        ServiceHelper.ConfigService("PortService", false);
                    }
                    if (!ServiceHelper.IsServiceExisted("PortService"))
                    {
                        ServiceHelper.ConfigService("PortService", true);
                    }
                    ServiceHelper.StartService("PortService");
                    goto StartLable;
                }
                else if (key == 2.ToString())
                {
                    if (!ServiceHelper.IsServiceExisted("PortService"))
                    {
                        ServiceHelper.ConfigService("PortService", true);
                    }
                    else
                    {
                        Console.WriteLine("\n服务已存在......");
                    }
                    goto StartLable;
                }
                else if (key == 3.ToString())
                {
                    if (ServiceHelper.IsServiceExisted("PortService"))
                    {
                        ServiceHelper.ConfigService("PortService", false);
                    }
                    else
                    {
                        Console.WriteLine("\n服务不存在......");
                    }
                    goto StartLable;
                }
                else if (key == 4.ToString())
                {
                    if (!ServiceHelper.IsServiceExisted("PortService"))
                    {
                        Console.WriteLine("\n服务不存在......");
                    }
                    else
                    {
                        Console.WriteLine("\n服务状态：" + ServiceHelper.GetServiceStatus("PortService").ToString());
                    }
                    goto StartLable;
                }
                else if (key == 5.ToString())
                {
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("\n请输入一个有效键！");
                    Console.WriteLine("————————————————————");
                    goto StartLable;
                }
            }
        }
    }
}