﻿using Hangfire;
using Hangfire.Console;
using Hangfire.RecurringJobExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bucket.HangFire.Server
{
    public class Startup
    {
        /// <summary>
        /// 初始化启动配置
        /// </summary>
        /// <param name="configuration">配置</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        /// <summary>
        /// 配置
        /// </summary>
        public IConfiguration Configuration { get; }
        /// <summary>
        /// 配置服务
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            // 注入Hangfire服务
            if (Configuration.GetValue<bool>("Scheduler:Enable"))
            {
                services.AddHangfire(build =>
                {
                    build.UseRedisStorage(Configuration.GetValue<string>("Scheduler:RedisServer"));
                    build.UseConsole();
                    build.UseRecurringJob("recurringjob.json");
                    build.UseDefaultActivator();
                });

            }
        }
        /// <summary>
        /// 开启Jobs工作
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (Configuration.GetValue<bool>("Scheduler:Enable"))
            {
                // 容器载入Scheduler服务
                HangfireServiceProvider.ServiceProvider = app.ApplicationServices;
                // 启动Scheduler服务
                app.UseHangfireServer(new BackgroundJobServerOptions
                {
                    ServerName = Configuration.GetValue<string>("Scheduler:ServerName")
                    // 注意队列名，进行任务隔离
                });
                // 启动Job界面UI
                app.UseHangfireDashboard(options: new DashboardOptions()
                {
                    // 本地测试可以不添加，linux下必添加
                    Authorization = new[] { new HangfireAuthorizationFilter() }
                });

                ///
                /// 特别注意，linux下运行时，可能存在"timezone": "China Standard Time"找不到的情况
                /// 需进行时区复制 cp /usr/share/zoneinfo/Asia/Shanghai /usr/share/zoneinfo/'China Standard Time'
                ///
            }
        }
    }
}
