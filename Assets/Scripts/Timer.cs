public struct Timer {
	public float EndTime;
	public float Duration;

	public float Tolerance;
	public static float DefaultTolerance = 0.0001f;

	public bool Enabled;

	public Timer(float duration, float tolerance, bool startReady = false) {
		Duration = duration;
		EndTime = startReady ? 0 : UnityEngine.Time.time + duration;
		Tolerance = tolerance;
		Enabled = true;
	}

	public Timer(float duration, bool startReady = false) : this(duration, DefaultTolerance, startReady) { }


	public void Reset() {
		EndTime = UnityEngine.Time.time + Duration;
		Enabled = true;
	}

	public void Stop() {
		EndTime = float.MaxValue;
		Enabled = false;
	}

	public bool IsDone(float currentTime) {
		return Enabled && currentTime >= EndTime - Tolerance;
	}

	public float TimeUntilDone(float currentTime) {
		return EndTime - Tolerance - currentTime;
	}

	public float TimeUntilDone() => TimeUntilDone(UnityEngine.Time.time);

	public bool IsDoneWithinSeconds(float currentTime, float doneWithinTime) {
		return Enabled && TimeUntilDone(currentTime) <= doneWithinTime;
	}

	public bool IsDoneWithinSeconds(float doneWithinTime) => Enabled && TimeUntilDone() <= doneWithinTime;

	public bool IsDone() => IsDone(UnityEngine.Time.time);

	public static implicit operator bool(Timer timer) {
		return timer.IsDone();
	}
}