import time
import cv2
import json
import threading
from detector import create_detector, detect
from debug_view import draw
from pipe_client import PipeClient

DEBUG = False


def settings_receiver(pipe: PipeClient, stop_event: threading.Event) -> None:
    while not stop_event.is_set():
        data = pipe.receive_settings()
        if data:
            print(f"[설정 수신] {data}", flush=True)


def main():
    pipe = PipeClient()
    print("[pipe] 서버 대기 중...", flush=True)
    pipe.connect_with_retry()

    detector = create_detector()
    cap = cv2.VideoCapture(0)
    timestamp_ms = 0

    stop_event = threading.Event()
    receiver_thread = threading.Thread(
        target=settings_receiver, args=(pipe, stop_event), daemon=True)
    receiver_thread.start()

    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            break

        timestamp_ms = int(time.time() * 1000)
        coords = detect(detector, frame, timestamp_ms)

        if not pipe.send(coords):
            print("[pipe] 연결 끊김, 재연결 시도...", flush=True)
            pipe.connect_with_retry()

        if DEBUG:
            draw(frame, coords)
            cv2.imshow("Dirtnithm Debug", frame)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

    stop_event.set()
    cap.release()
    detector.close()
    pipe.close()
    if DEBUG:
        cv2.destroyAllWindows()


if __name__ == "__main__":
    main()
