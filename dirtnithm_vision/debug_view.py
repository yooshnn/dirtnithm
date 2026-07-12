import cv2

THRESHOLD = 0.4  # 디버그용 임시 threshold


def draw(frame, coords: dict) -> None:
    """
    프레임에 손 위치를 그린다.
    coords: {"left": float | None, "right": float | None}
    """
    h, w, _ = frame.shape

    labels = {"left": "Left", "right": "Right"}
    colors = {"left": (255, 100, 0), "right": (0, 100, 255)}

    for side, label in labels.items():
        y_ratio = coords[side]
        if y_ratio is None:
            continue
        cy = int(y_ratio * h)
        cx = w // 4 if side == "left" else 3 * w // 4
        color = colors[side]
        cv2.circle(frame, (cx, cy), 10, color, -1)
        cv2.putText(frame, f"{label} {y_ratio:.2f}", (cx + 12, cy),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.6, color, 2)
