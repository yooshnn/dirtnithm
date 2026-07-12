a = Analysis(
    ['main.py'],
    pathex=['.'],
    binaries=[
        ('.venv/Lib/site-packages/mediapipe/tasks/c/libmediapipe.dll', 'mediapipe/tasks/c'),
    ],
    datas=[
        ('hand_landmarker.task', '.'),
        ('.venv/Lib/site-packages/mediapipe/modules', 'mediapipe/modules'),
    ],
    hiddenimports=[
        'mediapipe.tasks.c',
        'mediapipe.tasks.python.core.mediapipe_c_bindings',
    ],
    hookspath=[],
    runtime_hooks=[],
    excludes=[],
)

pyz = PYZ(a.pure)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.datas,
    name='dirtnithm_vision',
    debug=False,
    console=True,
    onefile=True,
)
