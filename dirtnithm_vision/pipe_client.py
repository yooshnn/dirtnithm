import json
import time
import threading
import win32pipe
import win32file

COORD_PIPE = r'\\.\pipe\dirtnithm_coords'
SETTINGS_PIPE = r'\\.\pipe\dirtnithm_settings'


class PipeClient:
    def __init__(self):
        self._coord_handle = None
        self._lock = threading.Lock()
        self._connected = False

    def connect(self) -> bool:
        try:
            self._coord_handle = win32file.CreateFile(
                COORD_PIPE,
                win32file.GENERIC_WRITE,
                0, None,
                win32file.OPEN_EXISTING,
                0, None
            )
            self._connected = True
            return True
        except Exception as e:
            print(f"[pipe] connect 예외: {e}", flush=True)
            self._connected = False
            return False

    def connect_with_retry(self, interval: float = 1.0) -> None:
        while not self._connected:
            if self.connect():
                print("[pipe] 연결됨", flush=True)
            else:
                time.sleep(interval)

    def send(self, data: dict) -> bool:
        if not self._connected or self._coord_handle is None:
            return False
        try:
            line = json.dumps(data) + '\n'
            with self._lock:
                win32file.WriteFile(self._coord_handle, line.encode('utf-8'))
            return True
        except Exception as e:
            print(f"[pipe] send 예외: {e}", flush=True)
            self._connected = False
            return False

    def receive_settings(self) -> dict | None:
        try:
            handle = win32file.CreateFile(
                SETTINGS_PIPE,
                win32file.GENERIC_READ,
                0, None,
                win32file.OPEN_EXISTING,
                0, None
            )
            _, data = win32file.ReadFile(handle, 4096)
            win32file.CloseHandle(handle)
            return json.loads(data.decode('utf-8').strip())
        except Exception:
            return None

    def close(self) -> None:
        if self._coord_handle:
            win32file.CloseHandle(self._coord_handle)
            self._coord_handle = None
            self._connected = False
