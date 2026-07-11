namespace Dirtnithm.App.Services;

public interface IKeyInputService
{
    void Press(ushort vk);
    void Release(ushort vk);
}
