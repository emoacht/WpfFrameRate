using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace WpfFrameRate
{
	public class MainWindowViewModel : ObservableObject
	{
		public IAsyncRelayCommand MeasureCommand => _measureCommand ??= new(() => MeasureFrameRateAsync(TimeSpan.FromSeconds(10)), () => !IsMeasuring);
		private AsyncRelayCommand? _measureCommand;

		public bool IsMeasuring
		{
			get => _isMeasuring;
			set
			{
				if (SetProperty(ref _isMeasuring, value))
					_measureCommand?.NotifyCanExecuteChanged();
			}
		}
		private bool _isMeasuring;

		public float? FrameRate
		{
			get => _frameRate;
			set => SetProperty(ref _frameRate, value);
		}
		private float? _frameRate;

		public async Task MeasureFrameRateAsync(TimeSpan duration)
		{
			try
			{
				IsMeasuring = true;

				// The way to measure frame rate is based on the discussion below.
				// https://stackoverflow.com/questions/5812384/why-is-frame-rate-in-wpf-irregular-and-not-limited-to-monitor-refresh

				var sw = new Stopwatch();

				int count = 0;
				TimeSpan renderingTime = default;
				void OnRendering(object? sender, EventArgs e)
				{
					if (sw.IsRunning)
					{
						var args = (RenderingEventArgs)e;
						if (renderingTime != args.RenderingTime)
						{
							renderingTime = args.RenderingTime;
							count++;
						}
					}
				}

				CompositionTarget.Rendering += OnRendering;

				sw.Start();
				await Task.Delay(duration);
				sw.Stop();

				CompositionTarget.Rendering -= OnRendering;
				FrameRate = count / (float)sw.Elapsed.TotalSeconds;
			}
			finally
			{
				IsMeasuring = false;
			}
		}
	}
}