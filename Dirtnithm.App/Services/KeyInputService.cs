using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace Dirtnithm.App.Services;

public class KeyInputService : IKeyInputService
{
    private readonly ILogger<KeyInputService> _logger;

    [DllImport("interception.dll")]
    private static extern nint interception_create_context();

    [DllImport("interception.dll")]
    private static extern void interception_destroy_context(nint context);

    [DllImport("interception.dll")]
    private static extern int interception_send(
        nint context,
        int device,
        ref InterceptionKeyStroke stroke,
        uint nstroke);

    [StructLayout(LayoutKind.Sequential)]
    private struct InterceptionKeyStroke
    {
        public ushort Code;
        public ushort State;
        public uint Information;
    }

    private const ushort INTERCEPTION_KEY_DOWN = 0x00;
    private const ushort INTERCEPTION_KEY_UP = 0x01;

    private nint _context;
    private int _device = 1;

    public KeyInputService(ILogger<KeyInputService> logger)
    {
        _logger = logger;
        _context = interception_create_context();
    }

    public void Press(ushort vk)
    {
        var scan = VkToScan(vk);
        var stroke = new InterceptionKeyStroke { Code = scan, State = INTERCEPTION_KEY_DOWN };
        interception_send(_context, _device, ref stroke, 1);
    }

    public void Release(ushort vk)
    {
        var scan = VkToScan(vk);
        var stroke = new InterceptionKeyStroke { Code = scan, State = INTERCEPTION_KEY_UP };
        interception_send(_context, _device, ref stroke, 1);
    }

    private static ushort VkToScan(ushort vk)
    {
        var scan = MapVirtualKey(vk, 0);
        return (ushort)scan;
    }

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    ~KeyInputService()
    {
        if (_context != nint.Zero)
        {
            interception_destroy_context(_context);
        }
    }
}
