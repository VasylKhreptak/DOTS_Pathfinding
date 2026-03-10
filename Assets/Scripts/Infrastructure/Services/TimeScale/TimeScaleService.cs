using Infrastructure.Services.TimeScale.Core;
using UnityEngine;

namespace Infrastructure.Services.TimeScale
{
    public class TimeScaleService : ITimeScaleService
    {
        public void PauseTime() => Time.timeScale = 0f;

        public void ResumeTime() => Time.timeScale = 1f;
    }
}