using Microsoft.JSInterop;
namespace ClientSideWASM.Audio {
    public class AudioService
    {
        private readonly IJSRuntime _js;
        private bool _isPlaying;

        public AudioService(IJSRuntime js)
        {
            _js = js;
        }

        public bool IsPlaying => _isPlaying;

        public async Task Play(string path)
        {
            await _js.InvokeVoidAsync("audioInterop.play", path);
            _isPlaying = true;
        }

        public async Task Stop()
        {
            await _js.InvokeVoidAsync("audioInterop.stop");
            _isPlaying = false;
        }

        public async Task Toggle(string path)
        {
            if (_isPlaying)
                await Stop();
            else
                await Play(path);
        }
    }
}