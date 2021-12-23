using OpenTK.Audio.OpenAL;

namespace GbaSnd;

public sealed class MCtx : IDisposable
{
    private ALDevice _dev;
    private ALContext _context;

    public MCtx()
    {
        _dev = ALC.OpenDevice(null);
        _context = ALC.CreateContext(_dev, new ALContextAttributes());
        if (_dev == null) throw new InvalidOperationException(AL.GetErrorString(AL.GetError()));
        try
        {
            if (!ALC.IsEnumerationExtensionPresent(_dev)) throw new NotSupportedException();
            if (!ALC.MakeContextCurrent(_context)) throw new InvalidOperationException(AL.GetErrorString(AL.GetError()));
        }
        catch
        {
            ALC.CloseDevice(_dev);
            _dev = default;
            throw;
        }
    }

    private void EnsureState()
    {
        if (_context.Handle == IntPtr.Zero) throw new ObjectDisposedException(nameof(MCtx));
    }

    public PCtx Stream(Stereo16StreamGenerator stereo16StreamGenerator, TextWriter? debug = null)
    {
        EnsureState();
        return new PCtx(stereo16StreamGenerator, debug);
    }

    private void ReleaseUnmanagedResources()
    {
        if (!ALC.MakeContextCurrent(default)) throw new InvalidOperationException(AL.GetErrorString(AL.GetError()));
        if (_context.Handle != IntPtr.Zero) ALC.DestroyContext(_context);
        _context = default;
        if (_dev.Handle != IntPtr.Zero) ALC.CloseDevice(_dev);
        _dev = default;
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~MCtx() => ReleaseUnmanagedResources();
}