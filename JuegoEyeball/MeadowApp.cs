﻿using JuegoEyeball;
using Meadow;
using Meadow.Devices;
using System;
using System.Threading.Tasks;
using WildernessLabs.Hardware.Juego;

namespace Froggit
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IJuegoHardware juego;

        EyeballController eyeballController;


        public override async Task Initialize()
        {
            Console.WriteLine("Initialize...");

            juego = Juego.Create();
            juego.StartButton.Clicked += StartButton_Clicked;

            juego.Left_UpButton.Clicked += Left_UpButton_Clicked;
            juego.Left_DownButton.Clicked += Left_DownButton_Clicked;
            juego.Left_LeftButton.Clicked += Left_LeftButton_Clicked;
            juego.Left_RightButton.Clicked += Left_RightButton_Clicked;

            eyeballController = new EyeballController(juego.Display);
        }

        private void Left_RightButton_Clicked(object sender, EventArgs e)
        {
        }

        private void Left_LeftButton_Clicked(object sender, EventArgs e)
        {
        }

        private void Left_DownButton_Clicked(object sender, EventArgs e)
        {
        }

        private void Left_UpButton_Clicked(object sender, EventArgs e)
        {
        }

        public override async Task Run()
        {
            Console.WriteLine("Run...");

            eyeballController.DrawEyeball();

            while (true)
            {
                eyeballController.Delay();
                eyeballController.RandomEyeMovement();
                eyeballController.Delay();
                eyeballController.CenterEye();
            }
        }

        private void StartButton_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("StartButton_Clicked");

        }
    }
}