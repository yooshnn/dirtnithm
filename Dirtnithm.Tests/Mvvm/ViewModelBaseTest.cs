using Dirtnithm.App.Mvvm;

namespace Dirtnithm.Tests.Mvvm;

public class ViewModelBaseTest
{
    private class TestViewModel : ViewModelBase
    {
        private int _value;
        public int Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }

    [Fact]
    public void SetProperty_WhenValueChanges_RaisesPropertyChanged()
    {
        const int initialValue = 5;
        const int newValue = 10;

        var vm = new TestViewModel { Value = initialValue };
        var raised = false;
        vm.PropertyChanged += (_, e) => raised = (e.PropertyName == nameof(TestViewModel.Value));

        vm.Value = newValue;

        Assert.True(raised);
    }

    [Fact]
    public void SetProperty_WhenValueUnchanged_DoesNotRaisePropertyChanged()
    {
        const int value = 5;

        var vm = new TestViewModel { Value = value };
        var raised = false;
        vm.PropertyChanged += (_, _) => raised = true;

        vm.Value = value;

        Assert.False(raised);
    }
}
