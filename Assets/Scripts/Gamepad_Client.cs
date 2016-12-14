using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;


public enum Brekel_BatteryType	{ BATTERY_TYPE_UNKNOWN, BATTERY_TYPE_DISCONNECTED, BATTERY_TYPE_WIRED, BATTERY_TYPE_ALKALINE, BATTERY_TYPE_NIMH	}
public enum Brekel_BatteryLevel { BATTERY_LEVEL_EMPTY, BATTERY_LEVEL_LOW, BATTERY_LEVEL_MEDIUM, BATTERY_LEVEL_FULL								}

[Serializable]
public struct Brekel_Gamepad
{
	public bool					isConnected;
	public Brekel_BatteryType	batteryType;
	public Brekel_BatteryLevel	batteryLevel;
	public bool					button_shoulder_L, button_shoulder_R, button_dpad_U, button_dpad_D, button_dpad_L, button_dpad_R, button_start, button_back, button_A, button_B, button_X, button_Y, button_thumb_L, button_thumb_R;
	public float				trigger_L, trigger_R, thumb_Lx, thumb_Ly, thumb_Rx, thumb_Ry;
}


public class Gamepad_Client : MonoBehaviour
{
	// settings
	[Header("Auto Discover Server IP")]
	[Tooltip("Automatically discover server on local network")]
	public	bool						autoDiscoverServer		= true;
	[Tooltip("Filter to only search for IPs starting with this string")]
	public	String						ipStartsWith			= "192.168.";

#if WINDOWS_UWP
    [Header("Fixed IP")]
	[Tooltip("Server hostname/IP to connect to (ignored when using Auto Discover)")]
	public	String						serverHostName			= "192.168.43.117";
#else
    [Header("Fixed IP")]
    [Tooltip("Server hostname/IP to connect to (ignored when using Auto Discover)")]
    public String serverHostName = "localhost";
#endif

    // data (you can access these from other scripts)
    [Header("Data")]
	public	Brekel_Gamepad[]			controllers				= new Brekel_Gamepad[4];
	private	bool						isConnected				= false;


	// DLL imports from C++ plugin
	[DllImport("Gamepad_Client")]	private static extern	void	Set_CPP_Debug_Function(IntPtr fp);
	[DllImport("Gamepad_Client")]	private static extern	bool	DiscoverServer(int timeoutSecs, String ipStartsWith);
	[DllImport("Gamepad_Client")]	private static extern	IntPtr	GetFoundIP();
	[DllImport("Gamepad_Client")]	private static extern	IntPtr	GetFoundAppName();
	[DllImport("Gamepad_Client")]	private static extern	void	Connect(String hostName, bool useTcp, int port);
	[DllImport("Gamepad_Client")]	private static extern	void	Disconnect();
	[DllImport("Gamepad_Client")]	private static extern	bool	GetIsConnected();
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_ControllerConnected(int id);
	[DllImport("Gamepad_Client")]	private static extern	int		Get_BatteryType(int id);
	[DllImport("Gamepad_Client")]	private static extern	int		Get_BatteryLevel(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_Shoulder_Left(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_Shoulder_Right(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_DPad_Up(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_DPad_Down(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_DPad_Left(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_DPad_Right(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_Start(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_Back(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_A(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_B(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_X(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_Y(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_Thumb_Left(int id);
	[DllImport("Gamepad_Client")]	private static extern	bool	Get_Button_Thumb_Right(int id);
	[DllImport("Gamepad_Client")]	private static extern	float	Get_Trigger_Left(int id);
	[DllImport("Gamepad_Client")]	private static extern	float	Get_Trigger_Right(int id);
	[DllImport("Gamepad_Client")]	private static extern	float	Get_Axis_Left_X(int id);
	[DllImport("Gamepad_Client")]	private static extern	float	Get_Axis_Left_Y(int id);
	[DllImport("Gamepad_Client")]	private static extern	float	Get_Axis_Right_X(int id);
	[DllImport("Gamepad_Client")]	private static extern	float	Get_Axis_Right_Y(int id);
	[DllImport("Gamepad_Client")]	private static extern	void	Set_DeadZone(float deadZone);
	[DllImport("Gamepad_Client")]	private static extern	void	Set_Vibrate(int id, float leftMotor, float rightMotor);

	//===================================================================
	// setup delegate for printing C++ debug messages to Unity's console
	//===================================================================
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void CPP_Debug_Delegate(string str);
	static void CPP_Debug(string str)
	{
		Debug.Log("C++: " + str);
	}


	//==========================================
	// called on game start, initializes things
	//==========================================
	void Start()
	{
		// setup delegate for printing C++ debug messages to Unity's console
		CPP_Debug_Delegate	callback_delegate	= new CPP_Debug_Delegate(CPP_Debug);
		IntPtr				intptr_delegate		= Marshal.GetFunctionPointerForDelegate(callback_delegate);
		Set_CPP_Debug_Function(intptr_delegate);

		// Discover server
		if(autoDiscoverServer)
		{
			Debug.Log("Auto Discovering Server");
			int		timeoutSecs	= 4;
			if(DiscoverServer(timeoutSecs, ipStartsWith))
			{
				String	foundIp		= Marshal.PtrToStringAnsi(GetFoundIP());
				String	foundApp	= Marshal.PtrToStringAnsi(GetFoundAppName());
				if(foundApp=="XBox_Controller_Server")
				{
					serverHostName = foundIp;
					Debug.Log("Server found on: " +serverHostName);
				}
				else
					Debug.Log("Server not of the right type");
			}
			else
				Debug.Log("Server not found");
		}
	

		// try to connect
		bool	useTCP	= true;
		int		port	= 9911;
		Connect(serverHostName, useTCP, port);
		isConnected	= GetIsConnected();
		if(!isConnected)
		{
			if(useTCP)	Debug.LogError("Cannot connect to: " + serverHostName + " on port: " + port + " using TCP/IP protocol");
			else		Debug.LogError("Cannot connect to: " + serverHostName + " on port: " + port + " using UDP protocol");
		}
		else
		{
			if(useTCP)	Debug.Log("Connected to: " + serverHostName + " on port: " + port + " using TCP/IP protocol");
			else		Debug.Log("Connected to: " + serverHostName + " on port: " + port + " using UDP protocol");
		}
	}

	
	//===========================================
	// called on game exit, cleans up everything
	//===========================================
    void OnDestroy()
    {
		Disconnect();
		Debug.Log("Disconnected from: " +serverHostName);
    }


	//=======================
	// called on every frame
	//=======================
	void Update()
	{
		// get fresh controller data
		for(int id=0; id<3; id++)
		{
			if(Get_ControllerConnected(id))
			{
				controllers[id].isConnected			= true;
				controllers[id].batteryType			= (Brekel_BatteryType)Get_BatteryType(id);
				controllers[id].batteryLevel		= (Brekel_BatteryLevel)Get_BatteryLevel(id);
				controllers[id].button_shoulder_L	= Get_Button_Shoulder_Left(id);
				controllers[id].button_shoulder_R	= Get_Button_Shoulder_Right(id);
				controllers[id].button_dpad_U		= Get_Button_DPad_Up(id);
				controllers[id].button_dpad_D		= Get_Button_DPad_Down(id);
				controllers[id].button_dpad_L		= Get_Button_DPad_Left(id);
				controllers[id].button_dpad_R		= Get_Button_DPad_Right(id);
				controllers[id].button_start		= Get_Button_Start(id);
				controllers[id].button_back			= Get_Button_Back(id);
				controllers[id].button_A			= Get_Button_A(id);
				controllers[id].button_B			= Get_Button_B(id);
				controllers[id].button_X			= Get_Button_X(id);
				controllers[id].button_Y			= Get_Button_Y(id);
				controllers[id].button_thumb_L		= Get_Button_Thumb_Left(id);
				controllers[id].button_thumb_R		= Get_Button_Thumb_Right(id);
				controllers[id].trigger_L			= Get_Trigger_Left(id);
				controllers[id].trigger_R			= Get_Trigger_Right(id);
				controllers[id].thumb_Lx			= Get_Axis_Left_X(id);
				controllers[id].thumb_Ly			= Get_Axis_Left_Y(id);
				controllers[id].thumb_Rx			= Get_Axis_Right_X(id);
				controllers[id].thumb_Ry			= Get_Axis_Right_Y(id);
			}
			else
			{
				controllers[id].isConnected				= false;
				controllers[id].batteryType				= Brekel_BatteryType.BATTERY_TYPE_DISCONNECTED;
				controllers[id].batteryLevel			= Brekel_BatteryLevel.BATTERY_LEVEL_EMPTY;
				controllers[id].button_shoulder_L		= false;
				controllers[id].button_shoulder_R		= false;
				controllers[id].button_dpad_U			= false;
				controllers[id].button_dpad_D			= false;
				controllers[id].button_dpad_L			= false;
				controllers[id].button_dpad_R			= false;
				controllers[id].button_start			= false;
				controllers[id].button_back				= false;
				controllers[id].button_A				= false;
				controllers[id].button_B				= false;
				controllers[id].button_X				= false;
				controllers[id].button_Y				= false;
				controllers[id].button_thumb_L			= false;
				controllers[id].button_thumb_R			= false;
				controllers[id].trigger_L				= 0f;
				controllers[id].trigger_R				= 0f;
				controllers[id].thumb_Lx				= 0f;
				controllers[id].thumb_Ly				= 0f;
				controllers[id].thumb_Rx				= 0f;
				controllers[id].thumb_Ry				= 0f;
			}
		}
	}


	//====================================
	// set vibration motors of controller
	//====================================
	public void SetVibrate(int id, float leftMotor, float rightMotor)
	{
		Set_Vibrate(id, leftMotor, rightMotor);
	}
}
