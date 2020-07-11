﻿using NDI;
using NewTek;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class SourceSelector : MonoBehaviour
{
    [SerializeField] Dropdown _dropdown = null;

    NdiReceiver _receiver = null;
    List<string> _sourceNames;
    bool _disableCallback;

    // HACK: Assuming that the dropdown has more than
    // three child objects only while it's opened.
    bool IsOpened => _dropdown.transform.childCount > 3;

    void Start()
    {
        WifiManager.GetInstance().SetupNetwork();
        _receiver = GetComponent<NdiReceiver>();

        Debug.Log(string.Format("NDI SDK Version {0}", Marshal.PtrToStringAnsi(NDIlib.version())));
    }

    void Update()
    {
        if (this._receiver == null) return;

        // Do nothing if the menu is opened.
        if (IsOpened) return;

        // NDI source name retrieval
        _sourceNames = NdiFinder.sourceNames.ToList();

        // Currect selection
        var index = _sourceNames.IndexOf(_receiver.ndiName);

        // Append the current name to the list if it's not found.
        if (index < 0)
        {
            index = _sourceNames.Count;
            _sourceNames.Add(_receiver.ndiName);
        }

        // Disable the callback while updating the menu options.
        _disableCallback = true;

        // Menu option update
        _dropdown.ClearOptions();
        _dropdown.AddOptions(_sourceNames);
        _dropdown.value = index;
        _dropdown.RefreshShownValue();

        // Resume the callback.
        _disableCallback = false;
    }

    public void OnChangeValue(int value)
    {
        if (_disableCallback) return;
        _receiver.ndiName = _sourceNames[value];
    }
}
