using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PryGuard.UI.Views;

namespace PryGuard.UI.ViewModels;
public static class ViewManager
{
    private static readonly Dictionary<BaseViewModel, Window> _windows = new Dictionary<BaseViewModel, Window>();

    public static Window Show([NotNull] BaseViewModel viewModel)
    {
        return Show(viewModel, null);
    }

    public static Window Show([NotNull] BaseViewModel viewModel, params object[] args)
    {
        Window window = CreateWindow(viewModel, args);
        window.Show();
        return window;
    }

    public static void ShowDialog([NotNull] BaseViewModel viewModel)
    {
        ShowDialog(viewModel, null);
    }

    public static void ShowDialog([NotNull] BaseViewModel viewModel, params object[] args)
    {
        CreateWindow(viewModel, args).ShowDialog();
    }

    private static Window CreateWindow([NotNull] BaseViewModel viewModel, params object[] args)
    {
        string fullName = viewModel.GetType().FullName;
        if (fullName == null)
            throw new ArgumentNullException("ViewManager , viewModel==null");
        Type type = Type.GetType(fullName.Replace("Model", "") ?? "", true);
        if (type == null)
            throw new ArgumentNullException("ViewManager , type==null");
        if (!(Activator.CreateInstance(type, args) is Window instance))
            throw new ArgumentNullException("ViewManager , view==null");
        instance.DataContext = viewModel;
        instance.Closed += (s, e) => viewModel.Close();
        if (!(instance is IBaseView baseView))
            throw new ArgumentException("ViewManager , ViewModel is not IBaseView");
        baseView.ViewModel = viewModel;
        _windows.Add(viewModel, instance);
        return instance;
    }

    public static bool Close(BaseViewModel viewModel)
    {
        if (!_windows.ContainsKey(viewModel))
            return false;
        Window window = _windows[viewModel];
        _windows.Remove(viewModel);
        window.Close();
        return true;
    }

    public static Dictionary<BaseViewModel, Window>.KeyCollection CurrentBrowsers()
    {
        return _windows.Keys;
    }
}
