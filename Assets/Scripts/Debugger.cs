using System;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour {
	public static bool ShowDebugging = false;

	private DebugType _debugType = DebugType.Console;
	private readonly List<LogData> _logInformationList = new List<LogData>();
	private int _currentLogIndex = -1;
	private int _infoLogCount;
	private int _warningLogCount;
	private int _errorLogCount;
	private int _fatalLogCount;
	private bool _showInfoLog = true;
	private bool _showWarningLog = true;
	private bool _showErrorLog = true;
	private bool _showFatalLog = true;
	private Vector2 _scrollLogView = Vector2.zero;
	private Vector2 _scrollCurrentLogView = Vector2.zero;
	private Vector2 _scrollSystemView = Vector2.zero;
	private bool _expansion;
	private Rect _windowRect = new Rect(16, 16, 100, 60);

	private int _fps;
	private Color _fpsColor = Color.white;
	private int _frameNumber;
	private float _lastShowFpsTime;

	private void Awake() {
		Application.logMessageReceived += LogHandler;
	}

	private void Update() {
		if(! ShowDebugging) return;
		_frameNumber += 1;
		float time = Time.realtimeSinceStartup - _lastShowFpsTime;
		if(! (time >= 1)) return;
		_fps = (int)(_frameNumber / time);
		_frameNumber = 0;
		_lastShowFpsTime = Time.realtimeSinceStartup;
	}

	private void OnDestory() {
		if(ShowDebugging) {
			Application.logMessageReceived -= LogHandler;
		}
	}

	private void LogHandler(string condition, string stackTrace, LogType type) {
		LogData log = new LogData {Time = DateTime.Now.ToString("HH:mm:ss"), Message = condition, StackTrace = stackTrace};

		switch(type) {
			case LogType.Assert:
				log.Type = "Fatal";
				_fatalLogCount += 1;
				break;
			case LogType.Exception:
			case LogType.Error:
				log.Type = "Error";
				_errorLogCount += 1;
				break;
			case LogType.Warning:
				log.Type = "Warning";
				_warningLogCount += 1;
				break;
			case LogType.Log:
				log.Type = "Info";
				_infoLogCount += 1;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(type), type, null);
		}

		_logInformationList.Add(log);

		if(_warningLogCount > 0) {
			_fpsColor = Color.yellow;
		}

		if(_errorLogCount > 0) {
			_fpsColor = Color.red;
		}
	}

	private void OnGUI() {
		if(! ShowDebugging) return;
		GlobalData.IsDragGui = GUIUtility.hotControl != 0;
		_windowRect = _expansion ? GUI.Window(0, _windowRect, ExpansionGuiWindow, "DEBUGGER") : GUI.Window(0, _windowRect, ShrinkGuiWindow, "DEBUGGER");
	}

	private void ExpansionGuiWindow(int windowId) {
		GUI.DragWindow(new Rect(0, 0, 10000, 20));

#region title

		GUILayout.BeginHorizontal();
		GUI.contentColor = _fpsColor;
		if(GUILayout.Button("FPS:" + _fps, GUILayout.Height(30))) {
			_expansion = false;
			_windowRect.width = 100;
			_windowRect.height = 60;
		}

		GUI.contentColor = (_debugType == DebugType.Console ? Color.white : Color.gray);
		if(GUILayout.Button("Console", GUILayout.Height(30))) {
			_debugType = DebugType.Console;
		}

		GUI.contentColor = (_debugType == DebugType.Memory ? Color.white : Color.gray);
		if(GUILayout.Button("Memory", GUILayout.Height(30))) {
			_debugType = DebugType.Memory;
		}

		GUI.contentColor = (_debugType == DebugType.System ? Color.white : Color.gray);
		if(GUILayout.Button("System", GUILayout.Height(30))) {
			_debugType = DebugType.System;
		}

		GUI.contentColor = (_debugType == DebugType.Screen ? Color.white : Color.gray);
		if(GUILayout.Button("Screen", GUILayout.Height(30))) {
			_debugType = DebugType.Screen;
		}

		GUI.contentColor = (_debugType == DebugType.Quality ? Color.white : Color.gray);
		if(GUILayout.Button("Quality", GUILayout.Height(30))) {
			_debugType = DebugType.Quality;
		}

		GUI.contentColor = (_debugType == DebugType.Environment ? Color.white : Color.gray);
		if(GUILayout.Button("Environment", GUILayout.Height(30))) {
			_debugType = DebugType.Environment;
		}

		GUI.contentColor = Color.white;
		GUILayout.EndHorizontal();

#endregion

#region console

		switch(_debugType) {
			case DebugType.Console: {
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("Clear")) {
					_logInformationList.Clear();
					_fatalLogCount = 0;
					_warningLogCount = 0;
					_errorLogCount = 0;
					_infoLogCount = 0;
					_currentLogIndex = -1;
					_fpsColor = Color.white;
				}

				GUI.contentColor = (_showInfoLog ? Color.white : Color.gray);
				_showInfoLog = GUILayout.Toggle(_showInfoLog, "Info [" + _infoLogCount + "]");
				GUI.contentColor = (_showWarningLog ? Color.white : Color.gray);
				_showWarningLog = GUILayout.Toggle(_showWarningLog, "Warning [" + _warningLogCount + "]");
				GUI.contentColor = (_showErrorLog ? Color.white : Color.gray);
				_showErrorLog = GUILayout.Toggle(_showErrorLog, "Error [" + _errorLogCount + "]");
				GUI.contentColor = (_showFatalLog ? Color.white : Color.gray);
				_showFatalLog = GUILayout.Toggle(_showFatalLog, "Fatal [" + _fatalLogCount + "]");
				GUI.contentColor = Color.white;
				GUILayout.EndHorizontal();

				_scrollLogView = GUILayout.BeginScrollView(_scrollLogView, "Box", GUILayout.Height(165));
				for(int i = 0; i < _logInformationList.Count; i ++) {
					bool show = false;
					Color color = Color.white;
					switch(_logInformationList[i].Type) {
						case "Fatal":
							show = _showFatalLog;
							color = Color.red;
							break;
						case "Error":
							show = _showErrorLog;
							color = Color.red;
							break;
						case "Info":
							show = _showInfoLog;
							color = Color.white;
							break;
						case "Warning":
							show = _showWarningLog;
							color = Color.yellow;
							break;
					}

					if(! show) continue;
					GUILayout.BeginHorizontal();
					if(GUILayout.Toggle(_currentLogIndex == i, "")) {
						_currentLogIndex = i;
					}

					GUI.contentColor = color;
					GUILayout.Label("[" + _logInformationList[i].Type + "] ");
					GUILayout.Label("[" + _logInformationList[i].Time + "] ");
					GUILayout.Label(_logInformationList[i].Message);
					GUILayout.FlexibleSpace();
					GUI.contentColor = Color.white;
					GUILayout.EndHorizontal();
				}

				GUILayout.EndScrollView();

				_scrollCurrentLogView = GUILayout.BeginScrollView(_scrollCurrentLogView, "Box", GUILayout.Height(100));
				if(_currentLogIndex != -1) {
					GUILayout.Label(_logInformationList[_currentLogIndex].Message + "\r\n\r\n" + _logInformationList[_currentLogIndex].StackTrace);
				}

				GUILayout.EndScrollView();
				break;
			}
			case DebugType.Memory: {
				GUILayout.BeginHorizontal();
				GUILayout.Label("Memory Information");
				GUILayout.EndHorizontal();

				GUILayout.BeginVertical("Box");
#if UNITY_5
            GUILayout.Label("总内存：" + Profiler.GetTotalReservedMemory() / 1000000 + "MB");
            GUILayout.Label("已占用内存：" + Profiler.GetTotalAllocatedMemory() / 1000000 + "MB");
            GUILayout.Label("空闲中内存：" + Profiler.GetTotalUnusedReservedMemory() / 1000000 + "MB");
            GUILayout.Label("总Mono堆内存：" + Profiler.GetMonoHeapSize() / 1000000 + "MB");
            GUILayout.Label("已占用Mono堆内存：" + Profiler.GetMonoUsedSize() / 1000000 + "MB");
#endif
#if UNITY_7
            GUILayout.Label("总内存：" + Profiler.GetTotalReservedMemoryLong() / 1000000 + "MB");
            GUILayout.Label("已占用内存：" + Profiler.GetTotalAllocatedMemoryLong() / 1000000 + "MB");
            GUILayout.Label("空闲中内存：" + Profiler.GetTotalUnusedReservedMemoryLong() / 1000000 + "MB");
            GUILayout.Label("总Mono堆内存：" + Profiler.GetMonoHeapSizeLong() / 1000000 + "MB");
            GUILayout.Label("已占用Mono堆内存：" + Profiler.GetMonoUsedSizeLong() / 1000000 + "MB");
#endif
				GUILayout.EndVertical();

				GUILayout.BeginHorizontal();
				if(GUILayout.Button("卸载未使用的资源")) {
					Resources.UnloadUnusedAssets();
				}

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if(GUILayout.Button("使用GC垃圾回收")) {
					GC.Collect();
				}

				GUILayout.EndHorizontal();
				break;
			}
			case DebugType.System:
				GUILayout.BeginHorizontal();
				GUILayout.Label("System Information");
				GUILayout.EndHorizontal();

				_scrollSystemView = GUILayout.BeginScrollView(_scrollSystemView, "Box");
				GUILayout.Label("操作系统：" + SystemInfo.operatingSystem);
				GUILayout.Label("系统内存：" + SystemInfo.systemMemorySize + "MB");
				GUILayout.Label("处理器：" + SystemInfo.processorType);
				GUILayout.Label("处理器数量：" + SystemInfo.processorCount);
				GUILayout.Label("显卡：" + SystemInfo.graphicsDeviceName);
				GUILayout.Label("显卡类型：" + SystemInfo.graphicsDeviceType);
				GUILayout.Label("显存：" + SystemInfo.graphicsMemorySize + "MB");
				GUILayout.Label("显卡标识：" + SystemInfo.graphicsDeviceID);
				GUILayout.Label("显卡供应商：" + SystemInfo.graphicsDeviceVendor);
				GUILayout.Label("显卡供应商标识码：" + SystemInfo.graphicsDeviceVendorID);
				GUILayout.Label("设备模式：" + SystemInfo.deviceModel);
				GUILayout.Label("设备名称：" + SystemInfo.deviceName);
				GUILayout.Label("设备类型：" + SystemInfo.deviceType);
				GUILayout.Label("设备标识：" + SystemInfo.deviceUniqueIdentifier);
				GUILayout.EndScrollView();
				break;
			case DebugType.Screen: {
				GUILayout.BeginHorizontal();
				GUILayout.Label("Screen Information");
				GUILayout.EndHorizontal();

				GUILayout.BeginVertical("Box");
				GUILayout.Label("DPI：" + Screen.dpi);
				GUILayout.Label("分辨率：" + Screen.currentResolution);
				GUILayout.EndVertical();

				GUILayout.BeginHorizontal();
				if(GUILayout.Button("全屏")) {
					Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, ! Screen.fullScreen);
				}

				GUILayout.EndHorizontal();
				break;
			}
			case DebugType.Quality: {
				GUILayout.BeginHorizontal();
				GUILayout.Label("Quality Information");
				GUILayout.EndHorizontal();

				GUILayout.BeginVertical("Box");
				string value = "";
				if(QualitySettings.GetQualityLevel() == 0) {
					value = " [最低]";
				} else if(QualitySettings.GetQualityLevel() == QualitySettings.names.Length - 1) {
					value = " [最高]";
				}

				GUILayout.Label("图形质量：" + QualitySettings.names[QualitySettings.GetQualityLevel()] + value);
				GUILayout.EndVertical();

				GUILayout.BeginHorizontal();
				if(GUILayout.Button("降低一级图形质量")) {
					QualitySettings.DecreaseLevel();
				}

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if(GUILayout.Button("提升一级图形质量")) {
					QualitySettings.IncreaseLevel();
				}

				GUILayout.EndHorizontal();
				break;
			}
			case DebugType.Environment: {
				GUILayout.BeginHorizontal();
				GUILayout.Label("Environment Information");
				GUILayout.EndHorizontal();

				GUILayout.BeginVertical("Box");
				GUILayout.Label("项目名称：" + Application.productName);
#if UNITY_5
            GUILayout.Label("项目ID：" + Application.bundleIdentifier);
#endif
#if UNITY_7
            GUILayout.Label("项目ID：" + Application.identifier);
#endif
				GUILayout.Label("项目版本：" + Application.version);
				GUILayout.Label("Unity版本：" + Application.unityVersion);
				GUILayout.Label("公司名称：" + Application.companyName);
				GUILayout.EndVertical();

				GUILayout.BeginHorizontal();
				if(GUILayout.Button("退出程序")) {
					Application.Quit();
				}

				GUILayout.EndHorizontal();
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
		}

#endregion
	}

	private void ShrinkGuiWindow(int windowId) {
		GUI.DragWindow(new Rect(0, 0, 10000, 20));

		GUI.contentColor = _fpsColor;
		if(GUILayout.Button("FPS:" + _fps, GUILayout.Width(80), GUILayout.Height(30))) {
			_expansion = true;
			_windowRect.width = 600;
			_windowRect.height = 360;
		}

		GUI.contentColor = Color.white;
	}
}

public struct LogData {
	public string Time;
	public string Type;
	public string Message;
	public string StackTrace;
}

public enum DebugType {
	Console,
	Memory,
	System,
	Screen,
	Quality,
	Environment
}
