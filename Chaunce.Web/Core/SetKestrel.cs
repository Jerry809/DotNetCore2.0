﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;

namespace Chaunce.Web
{
	/// <summary>
	/// set Kestrel
	/// </summary>
	internal class SetKestrel
	{
		/// <summary>
		/// configure Kestrel to Host
		/// </summary>
		/// <param name="options"></param>
		internal static void SetHost(Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions options)
		{
			var configuration = (IConfiguration)options.ApplicationServices.GetService(typeof(IConfiguration));
			var host = configuration.GetSection("RafHost").Get<Host>();//依据Host类反序列化appsettings.json中指定节点
			foreach (var endpointKvp in host.Endpoints)
			{
				var endpointName = endpointKvp.Key;
				var endpoint = endpointKvp.Value;//获取appsettings.json的相关配置信息
				if (!endpoint.IsEnabled)
				{
					continue;
				}

				var address = IPAddress.Parse(endpoint.Address);
				options.Listen(address, endpoint.Port, opt =>
				{
					if (endpoint.Certificate != null)//证书不为空使用UserHttps
					{
						switch (endpoint.Certificate.Source)
						{
							case "File":
								opt.UseHttps(endpoint.Certificate.Path, endpoint.Certificate.Password);
								break;
							default:
								throw new NotImplementedException($"文件 {endpoint.Certificate.Source}还没有实现");
						}
						//opt.UseConnectionLogging();
					}
				});

				options.UseSystemd();
			}
		}
	}

	#region 辅助model
	/// <summary>
	/// 待反序列化节点
	/// </summary>
	public class Host
	{
		/// <summary>
		/// appsettings.json字典
		/// </summary>
		public Dictionary<string, Endpoint> Endpoints { get; set; }
	}

	/// <summary>
	/// 终结点
	/// </summary>
	public class Endpoint
	{
		/// <summary>
		/// 是否启用
		/// </summary>
		public bool IsEnabled { get; set; }

		/// <summary>
		/// ip地址
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// 端口号
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		/// 证书
		/// </summary>
		public Certificate Certificate { get; set; }
	}

	/// <summary>
	/// 证书类
	/// </summary>
	public class Certificate
	{
		/// <summary>
		/// 源
		/// </summary>
		public string Source { get; set; }

		/// <summary>
		/// 证书路径()
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// 证书密钥
		/// </summary>
		public string Password { get; set; }
	}
	#endregion
}
