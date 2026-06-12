using ManagedBass;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace HanumanInstitute.MediaPlayer.Avalonia.Bass;

internal sealed class StreamWrapper : IDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private readonly GCHandle _selfHandle;
    private readonly FileProcedures _procedures;

    private bool _disposed;

    private StreamWrapper(Stream stream, bool leaveOpen)
    {
        _stream = stream;
        _leaveOpen = leaveOpen;

        _procedures = new FileProcedures
        {
            Close = Close,
            Length = Length,
            Read = Read,
            Seek = Seek
        };

        // Keep this wrapper alive until BASS releases it.
        _selfHandle = GCHandle.Alloc(this);
    }

    public static int CreateFromStream(
        Stream stream,
        bool leaveOpen = false,
        BassFlags flags = BassFlags.Default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
            throw new ArgumentException("Stream must be readable.", nameof(stream));

        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable.", nameof(stream));

        var wrapper = new StreamWrapper(stream, leaveOpen);

        int handle = ManagedBass.Bass.CreateStream(
            StreamSystem.NoBuffer,
            flags,
            wrapper._procedures,
            GCHandle.ToIntPtr(wrapper._selfHandle));

        if (handle == 0)
        {
            wrapper.Dispose();

            throw new InvalidOperationException(
                $"BASS error: {ManagedBass.Bass.LastError}");
        }

        return handle;
    }

    private long Length(IntPtr user)
    {
        return _stream.Length;
    }

    private bool Seek(long offset, IntPtr user)
    {
        try
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private unsafe int Read(IntPtr buffer, int length, IntPtr user)
    {
        try
        {
            return _stream.Read(new Span<byte>((void*)buffer, length));
        }
        catch
        {
            return -1;
        }
    }

    private void Close(IntPtr user)
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_selfHandle.IsAllocated)
            _selfHandle.Free();

        if (!_leaveOpen)
            _stream.Dispose();

        GC.SuppressFinalize(this);
    }

    ~StreamWrapper()
    {
        Dispose();
    }
}
