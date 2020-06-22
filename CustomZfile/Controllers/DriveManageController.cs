﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CustomZfile.Models;
using CustomZfile.Services;
using ClrLibCustomZfile;

namespace CustomZfile.Controllers
{
    [Route("/api")]
    [ApiController]
    public class DriveManageController : ControllerBase
    {

        [Route("/drives")]
        [HttpGet]
        public ResultBean DriveList()
		{
			return ResultBean.Success(SystemManager.ListAllDrives());
		}


		[Route("/drives/{id}")]
        [HttpGet]
        public ResultBean DriveItem(int id)
        {
            Console.WriteLine(id);
            DriveConfig driveConfig = SystemManager.GetDriveConfigById(id);
            return ResultBean.Success(driveConfig);
        }


        [Route("/drive")]
        [HttpPost]
        public ResultBean SaveDriveItem(Drive driveConfig)
        {
            int userId;
            if (int.TryParse(SystemManager.Decrypt(HttpContext.Request.Cookies["userId"]), out userId))
            {
                SystemManager.SaveNewDrive(driveConfig.name, userId);
                return ResultBean.Success();
            }
            return ResultBean.Error("userId error");
        }


        [Route("/drive/{id}")]
        [HttpDelete]
        public ResultBean DeleteDriveItem(int id)
        {
            int userId;
            if (int.TryParse(SystemManager.Decrypt(HttpContext.Request.Cookies["userId"]), out userId)){
                SystemManager.DeleteDriveById(id, userId);
                return ResultBean.Success();
            }
            return ResultBean.Error("userId error");
        }

    }
}