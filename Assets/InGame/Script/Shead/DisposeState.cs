//日本語対応
using UnityEngine;
using System;

public abstract class DisposeState : IDisposable
{
    private bool _isDisoised;

    public void Dispose()
    {
        if (_isDisoised)
            throw new ObjectDisposedException(nameof(DisposeState));

        DisposeInternal();
        _isDisoised = true;
    }

    protected abstract void DisposeInternal();
}
