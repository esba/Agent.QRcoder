/********************
 * Created by Esben Bast (http://goo.gl/hkMkvJ)
 * 
 * Big thank you goes to "Erik" and "micjahn"
 * See more on the AGENT forum: http://goo.gl/UmNLc5
 * 
 * This is not a complete app, but it works fine in its current form.
 * 
 * Use this source code freely, but please give credit and share your source on the AGENT forum.
 * 
 ********************/

using AGENT.Contrib.Hardware;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation.Media;
using System;
using System.Threading;

namespace Agent.QRcoder
{
    public class Program
    {
        static string _vcardTest =
            @"BEGIN:VCARD
VERSION:2.1
N;LANGUAGE=da:Doe;John;D.
FN:John D. Doe
ORG:MyCompany
TITLE:Consultant
TEL;WORK;VOICE:+45 12 34 56 78
TEL;CELL;VOICE:+45 87 65 43 21
EMAIL;PREF;INTERNET:john@example.com
END:VCARD";

        static Bitmap _display;
        static readonly Font FONT = Resources.GetFont(Resources.FontResources.small);
        static bool _loading = true;

        static QRitems _QRcodes = new QRitems();

        public static void Main()
        {
            ButtonHelper.ButtonSetup = new Buttons[] { Buttons.TopRight, Buttons.BottomRight, Buttons.MiddleRight };
            ButtonHelper.Current.OnButtonPress += Current_OnButtonPress; // Do something when a button is pressed

            _display = new Bitmap(Bitmap.MaxWidth, Bitmap.MaxHeight);

            PopulateQRList(); // Get the list of possible QR codes

            _display.DrawRectangle(Color.White, 0, 0, 0, _display.Width, _display.Height, 0, 0, Color.White, 0, 0, Color.White, _display.Width, _display.Height, 255); // Set the background to white

            /* Write usage instructions to screen */
            _display.DrawText("Press for previous -->", FONT, Color.Black, 2, 0);
            _display.DrawText("Reload curent -->", FONT, Color.Black, 2, 58);
            _display.DrawText("Press for next -->", FONT, Color.Black, 2, 116);
            _display.Flush();

            // go to sleep; all further code should be timer-driven or event-driven
            Thread.Sleep(Timeout.Infinite);
        }

        /* Should be populated from phone app or other source */
        static void PopulateQRList()
        {
            // Test populating
            _QRcodes.AddQRcode("No1", "Hello1!");
            _QRcodes.AddQRcode("No2: vCard", _vcardTest);
            _QRcodes.AddQRcode("No3", "Hello3!");
            _QRcodes.AddQRcode("No4", "Hello4!");
            _QRcodes.AddQRcode("No5", "Hello5!");
        }

        static void UpdateDisplay()
        {
            _display.DrawRectangle(Color.White, 0, 0, 0, _display.Width, _display.Height, 0, 0, Color.White, 0, 0, Color.White, _display.Width, _display.Height, 255); // Set the background to white

            // Create new thread for generating the QR code and starts it
            Thread QRCreator = new Thread(() => DoQRCode());
            QRCreator.Start();

            _loading = true; // Starts loading animation

            /* Blinks "Loading..." on the display while the QR is being generated */
            while (_loading) // Runs while the QR is being generated
            {
                _display.DrawText("Loading... " + _QRcodes.GetCurrent().Name, FONT, Color.Black, 2, 115);
                _display.Flush();
                Thread.Sleep(500);
                _display.DrawText("Loading... " + _QRcodes.GetCurrent().Name, FONT, Color.White, 2, 115);
                _display.Flush();
                Thread.Sleep(500);
            }
        }

        static void DoQRCode()
        {
            ZXing.QrCode.QRCodeWriter writer = new ZXing.QrCode.QRCodeWriter();

            /* QR options */
            System.Collections.Hashtable hints = new System.Collections.Hashtable();
            hints.Add(ZXing.EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.L); // Sets error correction to low, which enables more data to be embedded in the QR code
            hints.Add(ZXing.EncodeHintType.MARGIN, 1); // Sets smallest margin to make the QR code bigger

            ZXing.Common.BitMatrix matrix = writer.encode(_QRcodes.GetCurrent().Content, ZXing.BarcodeFormat.QR_CODE, _display.Width, _display.Height, hints); // Creates the matrix which contains the QR code (may take a lot of time)

            _loading = false; // Ends loading animation

            /* Writes the matrix directly to the screen */
            for (int y = 0; y < matrix.Height; y++)
            {
                for (int x = 0; x < matrix.Width; x++)
                {
                    Color color = matrix[x, y] ? Color.Black : Color.White;
                    _display.SetPixel(x, y, color);
                }
                _display.Flush(); // Update display line by line
            }
            PersistQRCode(); // Not implemented
        }

        static void PersistQRCode()
        {
            // Store last generated QR code (Bitmap._display) in watch memory and load it when app is reopened
        }

        /* Use buttons to switch or reload QR codes */
        static void Current_OnButtonPress(Buttons button, InterruptPort port, ButtonDirection direction, DateTime time)
        {
            if (button == Buttons.BottomRight && direction == ButtonDirection.Up)
            {
                _QRcodes.GetNext();
                UpdateDisplay();
            }

            if (button == Buttons.TopRight && direction == ButtonDirection.Up)
            {
                _QRcodes.GetPrevious();
                UpdateDisplay();
            }

            if (button == Buttons.MiddleRight && direction == ButtonDirection.Up)
            {
                UpdateDisplay();
            }
        }
    }
}
