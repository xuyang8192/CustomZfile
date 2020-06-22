﻿using CustomZfile.Models;
using CustomZfile.DbTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using ClrLibCustomZfile;


namespace CustomZfile.Services
{
	public static class SystemManager
	{
		private static CustomZfileDbContext context = new CustomZfileDbContext();

		public const string WwwRoot = "wwwroot";
		public const string FileRoot = "fileroot";
		public const string BasePath = "wwwroot/fileroot";

		private const string EncryptionAssemblyName = "GlobalAssemblyCustomZfile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=747f6a76d53f4723";

		public static string StartTime { get; } = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

		private static SystemConfig systemConfig;

		private static CustomZfileDbContext dbContext = new CustomZfileDbContext(); 

		static SystemManager()
		{
			systemConfig = new SystemConfig();
		}

		public static DriveConfig GetDriveConfigById(int id)
		{
			return systemConfig.driveConfigs[id];
		}

		public static List<DriveConfig> ListAllDrives()
		{
			DirectoryInfo dirInfo = new DirectoryInfo(BasePath);
			List<DriveConfig> driveConfigs = new List<DriveConfig>();

			int id = 0;
			foreach (DirectoryInfo di in dirInfo.GetDirectories())
			{
				driveConfigs.Add(new DriveConfig(id, di.Name, di.CreationTime));
				id++;
			}

			return driveConfigs;
		}

		
		public static int SaveNewDrive(string driveName, int userId)
		{
			Drive newDrive = new Drive { name = driveName, creator_id = userId };
			var re = context.drive.Add(newDrive);
			context.SaveChanges();
			LocalFileManager.CreateDir(FileRoot + re.Entity.id.ToString());
			return re.Entity.id;
		}

		public static bool DeleteDriveById(int driveId, int userId)
		{
			if (!CheckDeletePermission(driveId, userId))
			{
				return false;
			}
			LocalFileManager.DelDir(FileRoot + driveId.ToString());
			context.Remove(new Drive { id = driveId });
			context.SaveChanges();
			return true;
		}


		private static bool CheckDeletePermission(int driveId, int userId)
		{
			var entity = from d in context.drive
						where d.id == driveId && d.creator_id == userId
						select d;

			return entity != null;
		}

		public static SystemConfig GetSystemConfig()
		{
			return systemConfig;
		}

		public static User UserExist(string username, string password)
		{
			var user = (from u in context.user
						where u.username == username && u.password == password
						select u).ToList();
			if (user.Count == 0)
			{
				return null;
			}
			return user[0];
		}

		// Implement by assembly in GAC.
		public static string Encrypt(string str)
		{
			var asm = Assembly.Load(EncryptionAssemblyName);
			Type t = asm.GetType("EncryptionLib.Encryption");

			if (t == null)
			{
				throw new Exception("No such class exists.");
			}

			var methodInfoStatic = t.GetMethod("AesEncrypt");
			if (methodInfoStatic == null)
			{
				throw new Exception("No such static method exists.");
			}

			object[] staticParameters = new object[1];
			staticParameters[0] = str;

			// Invoke static method
			return (string)methodInfoStatic.Invoke(null, staticParameters);
		}

		// Implement by assembly in GAC.
		public static string Decrypt(string str)
		{
			var asm = Assembly.Load(EncryptionAssemblyName);
			Type t = asm.GetType("EncryptionLib.Encryption");
			
			if (t == null)
			{
				throw new Exception("No such class exists.");
			}

			var methodInfoStatic = t.GetMethod("AesDecrypt");
			if (methodInfoStatic == null)
			{
				throw new Exception("No such static method exists.");
			}

			object[] staticParameters = new object[1];
			staticParameters[0] = str;

			// Invoke static method
			return (string)methodInfoStatic.Invoke(null, staticParameters);
		}

	}
}