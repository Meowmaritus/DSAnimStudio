using System;
using Linearstar.Windows.RawInput;
using System.Windows.Forms;
using ImGuiNET;

namespace DSAnimStudio
{
	class RawInputEventArgs : EventArgs
	{
		public RawInputEventArgs(RawInputData data)
		{
			Data = data;
		}

		public RawInputData Data { get; }
	}

	class RawInputReceiverWindow : NativeWindow
	{
		public event EventHandler<RawInputEventArgs> Input;

		public RawInputReceiverWindow()
		{
			CreateHandle(new CreateParams
			{
				X = 0,
				Y = 0,
				Width = 0,
				Height = 0,
				Style = 0x800000,
			});
		}

		protected override void WndProc(ref Message m)
		{
			const int WM_INPUT = 0x00FF;
			const int WM_CHAR = 0x0102;

			if (m.Msg == WM_INPUT)
			{
				var data = RawInputData.FromHandle(m.LParam);

				Input?.Invoke(this, new RawInputEventArgs(data));
			}
			else if (m.Msg == WM_CHAR)
			{
				ImGui.GetIO().AddInputCharacter((uint)m.WParam.ToInt64());
			}

			base.WndProc(ref m);
		}
	}

	class RawMouseMovedEventArgs : EventArgs
	{
		public readonly int X;
		public readonly int Y;
		public RawMouseMovedEventArgs(int x, int y)
		{
			X = x;
			Y = y;
		}
	}

	public static class WindowsMouseHook
    {
		static RawInputReceiverWindow receiver;

		public delegate void RawMouseMovedDelegate(int x, int y);

		public static RawMouseMovedDelegate RawMouseMoved;

		static void RawInputReceived(object sender, RawInputEventArgs e)
		{
			// Catch your input here!
			var data = e.Data;

			// You can identify the source device using Header.DeviceHandle or just Device.
			var sourceDeviceHandle = data.Header.DeviceHandle;
			var sourceDevice = data.Device;

			// The data will be an instance of either RawInputMouseData, RawInputKeyboardData, or RawInputHidData.
			// They contain the raw input data in their properties.
			switch (data)
			{
				case RawInputMouseData mouse:
					//if (mouse.Mouse.Flags != Linearstar.Windows.RawInput.Native.RawMouseFlags.None)
					//{
					//	RawMouseMoved?.Invoke(mouse.Mouse.LastX, mouse.Mouse.LastY);
					//}
					RawMouseMoved?.Invoke(mouse.Mouse.LastX, mouse.Mouse.LastY);
					break;
			}
		}

		//public static void RefreshHook()
		//{
		//	receiver.Input -= RawInputReceived;
		//	receiver.DestroyHandle();
		//	receiver = new RawInputReceiverWindow();
		//	receiver.Input += RawInputReceived;
		//}

		public static void Hook(IntPtr hWnd)
        {
			// To begin catching inputs, first make a window that listens WM_INPUT.
			receiver = new RawInputReceiverWindow();

			RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.InputSink, receiver.Handle);

			receiver.Input += RawInputReceived;
		}

		public static void Unhook()
		{
			receiver?.ReleaseHandle();
			RawInputDevice.UnregisterDevice(HidUsageAndPage.Mouse);
		}
    }
}