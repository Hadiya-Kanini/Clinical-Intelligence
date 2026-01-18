"""Rate limiter for API calls with configurable RPM limit."""

import time
from typing import Callable, Optional


class RateLimiter:
    """
    Simple RPM (requests per minute) rate limiter using token bucket algorithm.
    
    Ensures the worker never exceeds the configured RPM limit for API calls.
    Supports injection of clock/sleeper for deterministic testing.
    """
    
    def __init__(
        self,
        rpm_limit: int = 15,
        clock: Optional[Callable[[], float]] = None,
        sleeper: Optional[Callable[[float], None]] = None
    ):
        """
        Initialize rate limiter.
        
        Args:
            rpm_limit: Maximum requests per minute (default 15 for free tier).
            clock: Optional clock function returning current time in seconds.
            sleeper: Optional sleep function for waiting.
        """
        self._rpm_limit = rpm_limit
        self._clock = clock or time.time
        self._sleeper = sleeper or time.sleep
        
        self._interval = 60.0 / rpm_limit
        self._last_request_time: Optional[float] = None
    
    @property
    def rpm_limit(self) -> int:
        """Get the configured RPM limit."""
        return self._rpm_limit
    
    @property
    def interval_seconds(self) -> float:
        """Get the minimum interval between requests in seconds."""
        return self._interval
    
    def acquire(self) -> None:
        """
        Acquire permission to make a request, blocking if necessary.
        
        Blocks until enough time has passed since the last request
        to stay within the RPM limit.
        """
        current_time = self._clock()
        
        if self._last_request_time is not None:
            elapsed = current_time - self._last_request_time
            wait_time = self._interval - elapsed
            
            if wait_time > 0:
                self._sleeper(wait_time)
                current_time = self._clock()
        
        self._last_request_time = current_time
    
    def reset(self) -> None:
        """Reset the rate limiter state."""
        self._last_request_time = None
