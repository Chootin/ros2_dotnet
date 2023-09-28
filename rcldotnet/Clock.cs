/* Copyright 2023 Queensland University of Technology.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Runtime.InteropServices;
using builtin_interfaces.msg;
using ROS2.Utils;

namespace ROS2
{
    public static class TimeConstants
    {
        public const long SECONDS_TO_NANOSECONDS = 1000L * 1000L * 1000L;
    }

    /// <summary>
    /// Time source type, used to indicate the source of a time measurement.
    ///
    /// ROSTime will report the latest value reported by a ROS time source, or
    /// if a ROS time source is not active it reports the same as RCL_SYSTEM_TIME.
    /// For more information about ROS time sources, refer to the design document:
    /// http://design.ros2.org/articles/clock_and_time.html
    ///
    /// SystemTime reports the same value as the system clock.
    ///
    /// SteadyTime reports a value from a monotonically increasing clock.
    /// </summary>
    public enum ClockType
    {
        ClockUninitialized = 0,
        ROSTime,
        SystemTime,
        SteadyTime
    }

    /// Enumeration to describe the type of time jump.
    /// see definition here: https://github.com/ros2/rcl/blob/master/rcl/include/rcl/time.h
    public enum ClockChange
    {
        /// The source before and after the jump is ROS_TIME.
        RosTimeNoChange = 1,
        /// The source switched to ROS_TIME from SYSTEM_TIME.
        RosTimeActivated = 2,
        /// The source switched to SYSTEM_TIME from ROS_TIME.
        RosTimeDeactivated = 3,
        /// The source before and after the jump is SYSTEM_TIME.
        SystemTimeNoChange = 4
    }

    public struct TimeJump
    {
        public ClockChange clockChange;
        public Duration delta;
    }

    public delegate void JumpCallback(TimeJump timeJump, bool beforeJump);

    public struct JumpThreshold
    {
        public bool onClockChange;
        public Duration minForward;
        public Duration minBackward;

        public JumpThreshold(bool onClockChange, double minForwardSeconds, double minBackwardSeconds)
        {
            this.onClockChange = onClockChange;
            minForward = new Duration(minForwardSeconds);
            minBackward = new Duration(minBackwardSeconds);
        }
    }

    public struct TimePoint
    {

        public long nanoseconds;

        public Time ToMsg()
        {
            long sec = nanoseconds / TimeConstants.SECONDS_TO_NANOSECONDS;
            long nanosec = nanoseconds - (sec * TimeConstants.SECONDS_TO_NANOSECONDS);
            return new Time
            {
                Sec = (int)sec,
                Nanosec = (uint)nanosec
            };
        }

        public static TimePoint FromMsg(Time message)
        {
            return new TimePoint
            {
                nanoseconds = message.Sec * TimeConstants.SECONDS_TO_NANOSECONDS + message.Nanosec
            };
        }
    }

    public struct Duration
    {
        public long nanoseconds;

        public Duration(double seconds)
        {
            nanoseconds = (long)(seconds * TimeConstants.SECONDS_TO_NANOSECONDS);
        }
    }

    internal static class ClockDelegates
    {
        private static readonly DllLoadUtils _dllLoadUtils;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate RCLRet NativeRCLClockFunctionType(SafeClockHandle clockHandle);

        internal static NativeRCLClockFunctionType native_rcl_enable_ros_time_override = null;

        internal static NativeRCLClockFunctionType native_rcl_disable_ros_time_override = null;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate RCLRet NativeRCLClockGetNowType(SafeClockHandle clockHandle, out TimePoint time);

        internal static NativeRCLClockGetNowType native_rcl_clock_get_now = null;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate RCLRet NativeRCLSetRosTimeOverrideType(SafeClockHandle clockHandle, long timePointValue);

        internal static NativeRCLSetRosTimeOverrideType native_rcl_set_ros_time_override = null;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate RCLRet NativeRCLAddJumpCallbackType(SafeClockHandle clockHandle, JumpThreshold threshold, JumpCallback callback);

        internal static NativeRCLAddJumpCallbackType native_rcl_clock_add_jump_callback = null;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate RCLRet NativeRCLRemoveJumpCallbackType(SafeClockHandle clockHandle, JumpCallback callback);

        internal static NativeRCLRemoveJumpCallbackType native_rcl_clock_remove_jump_callback = null;

        static ClockDelegates()
        {
            _dllLoadUtils = DllLoadUtilsFactory.GetDllLoadUtils();
            IntPtr nativeLibrary = _dllLoadUtils.LoadLibrary("rcldotnet");

            _dllLoadUtils.RegisterNativeFunction(nativeLibrary, nameof(native_rcl_enable_ros_time_override), out native_rcl_enable_ros_time_override);
            _dllLoadUtils.RegisterNativeFunction(nativeLibrary, nameof(native_rcl_disable_ros_time_override), out native_rcl_disable_ros_time_override);
            _dllLoadUtils.RegisterNativeFunction(nativeLibrary, nameof(native_rcl_clock_get_now), out native_rcl_clock_get_now);
            _dllLoadUtils.RegisterNativeFunction(nativeLibrary, nameof(native_rcl_set_ros_time_override), out native_rcl_set_ros_time_override);
            _dllLoadUtils.RegisterNativeFunction(nativeLibrary, nameof(native_rcl_clock_add_jump_callback), out native_rcl_clock_add_jump_callback);
            _dllLoadUtils.RegisterNativeFunction(nativeLibrary, nameof(native_rcl_clock_remove_jump_callback), out native_rcl_clock_remove_jump_callback);
        }
    }

    public sealed class Clock
    {

        internal Clock(SafeClockHandle handle)
        {
            Handle = handle;
        }

        internal SafeClockHandle Handle { get; }

        public Time Now()
        {
            RCLRet ret = ClockDelegates.native_rcl_clock_get_now(Handle, out TimePoint timePoint);

            RCLExceptionHelper.CheckReturnValue(ret, $"{nameof(ClockDelegates.native_rcl_clock_get_now)}() failed.");

            return timePoint.ToMsg();
        }

        internal RCLRet EnableRosTimeOverride()
        {
            RCLRet ret = ClockDelegates.native_rcl_enable_ros_time_override(Handle);

            RCLExceptionHelper.CheckReturnValue(ret, $"{nameof(ClockDelegates.native_rcl_enable_ros_time_override)}() failed.");

            return ret;
        }

        internal RCLRet DisableRosTimeOverride()
        {
            RCLRet ret = ClockDelegates.native_rcl_disable_ros_time_override(Handle);

            RCLExceptionHelper.CheckReturnValue(ret, $"{nameof(ClockDelegates.native_rcl_disable_ros_time_override)}() failed.");

            return ret;
        }

        internal RCLRet SetRosTimeOverride(long timePointValue)
        {
            RCLRet ret = ClockDelegates.native_rcl_set_ros_time_override(Handle, timePointValue);

            RCLExceptionHelper.CheckReturnValue(ret, $"{nameof(ClockDelegates.native_rcl_set_ros_time_override)}() failed.");

            return ret;
        }

        public void AddJumpCallback(JumpThreshold threshold, JumpCallback callback)
        {
            RCLRet ret = ClockDelegates.native_rcl_clock_add_jump_callback(Handle, threshold, callback);

            RCLExceptionHelper.CheckReturnValue(ret, $"{nameof(ClockDelegates.native_rcl_clock_add_jump_callback)}() failed.");
        }

        public void RemoveJumpCallback(JumpCallback callback)
        {
            RCLRet ret = ClockDelegates.native_rcl_clock_remove_jump_callback(Handle, callback);

            RCLExceptionHelper.CheckReturnValue(ret, $"{nameof(ClockDelegates.native_rcl_clock_remove_jump_callback)}() failed.");
        }
    }
}
