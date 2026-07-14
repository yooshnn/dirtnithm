import urllib.request
import os
import cv2
import mediapipe as mp
from model import MODEL_PATH, ensure_model
from mediapipe.tasks import python
from mediapipe.tasks.python import vision

LANDMARK_INDEX = 12 # 0: 손목, 12: 중지 끝

def create_detector() -> vision.HandLandmarker:
    ensure_model()
    options = vision.HandLandmarkerOptions(
        base_options=python.BaseOptions(model_asset_path=MODEL_PATH),
        running_mode=vision.RunningMode.VIDEO,
        num_hands=2,
        min_hand_detection_confidence=0.7,
        min_hand_presence_confidence=0.5,
        min_tracking_confidence=0.5,
    )
    return vision.HandLandmarker.create_from_options(options)


def detect(detector: vision.HandLandmarker, frame, timestamp_ms: int) -> dict:
    """
    프레임에서 양손 Y좌표를 추출한다.
    반환: {"left": float | None, "right": float | None}
    """
    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb)
    result = detector.detect_for_video(mp_image, timestamp_ms)

    left_y = None
    right_y = None

    if result.hand_landmarks and result.handedness:
        for landmarks, handedness in zip(result.hand_landmarks, result.handedness):
            label = handedness[0].category_name
            y = round(landmarks[LANDMARK_INDEX].y, 4)
            if label == 'Left':
                left_y = y
            else:
                right_y = y

    return {"left": left_y, "right": right_y}