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

    public struct TimePoint
    {
        private const long SECONDS_TO_NANOSECONDS = 1000L * 1000L * 1000L;

        public long nanoseconds;

        public Time ToMsg()
        {
            long sec = nanoseconds / SECONDS_TO_NANOSECONDS;
            long nanosec = nanoseconds - (sec * SECONDS_TO_NANOSECONDS);
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
                nanoseconds = message.Sec * SECONDS_TO_NANOSECONDS + message.Nanosec
            };
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

        static ClockDelegates()
        {
            _dllLoadUtils = DllLoadUtilsFactory.GetDllLoadUtils();
            IntPtr nativeLibrary = _dllLoadUtils.LoadLibrary("rcldotnet");

            _dllLoadUtils.RegisterNativeFunction(nativeLibrary, nameof(native_rcl_enable_ros_time_override), out native_rcl_enable_ros_time_override);
            _dllLoadUtils.RegisterNativeFunction(nativeLibrary, nameof(native_rcl_disable_ros_time_override), out native_rcl_disable_ros_time_override);
            _dllLoadUtils.RegisterNativeFunction(nativeLibrary, nameof(native_rcl_clock_get_now), out native_rcl_clock_get_now);
            _dllLoadUtils.RegisterNativeFunction(nativeLibrary, nameof(native_rcl_set_ros_time_override), out native_rcl_set_ros_time_override);
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
    }
}
