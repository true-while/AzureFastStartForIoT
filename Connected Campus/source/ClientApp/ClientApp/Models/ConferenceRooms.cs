using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace ClientApp.Models
{

    public class ConferenceRoomDataPoint : INotifyPropertyChanged
    {
        public ConferenceRoomDataPoint(string DeviceId, string RoomStatus, string RoomTemp, string RoomAlt, string Color, string Time, string LightStatus, string RoomPressure)
        {
            _deviceId = DeviceId;
            _roomStatus = RoomStatus;
            _roomTemp = RoomTemp;
            _roomAlt = RoomAlt;
            _color = Color;
            _time = Time;
            _lightStatus = LightStatus;
            _roomPressure = RoomPressure;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        string _deviceId;

        public string DeviceId
        {
            get
            {
                return this._deviceId;
            }

            set
            {
                if (value != this._deviceId)
                {
                    this._deviceId = value;
                    NotifyPropertyChanged("DeviceId");
                }
            }
        }

        string _roomStatus;

        public string RoomStatus
        {
            get
            {
                return this._roomStatus;
            }

            set
            {
                if (value != this._roomStatus)
                {
                    this._roomStatus = value;
                    NotifyPropertyChanged("RoomStatus");
                }
            }
        }

        string _roomTemp;

        public string RoomTemp
        {
            get
            {
                return this._roomTemp;
            }

            set
            {
                if (value != this._roomTemp)
                {
                    this._roomTemp = value;
                    NotifyPropertyChanged("RoomTemp");
                }
            }
        }

        string _roomAlt;

        public string RoomAlt
        {
            get
            {
                return this._roomAlt;
            }

            set
            {
                if (value != this._roomAlt)
                {
                    this._roomAlt = value;
                    NotifyPropertyChanged("RoomAlt");
                }
            }
        }

        string _color;

        public string Color
        {
            get
            {
                return this._color;
            }

            set
            {
                if (value != this._color)
                {
                    this._color = value;
                    NotifyPropertyChanged("Color");
                }
            }
        }
        string _time;

        public string Time
        {
            get
            {
                return this._time;
            }

            set
            {
                if (value != this._time)
                {
                    this._time = value;
                    NotifyPropertyChanged("Time");
                }
            }
        }
        string _lightStatus;

        public string LightStatus
        {
            get
            {
                return this._lightStatus;
            }

            set
            {
                if (value != this._lightStatus)
                {
                    this._lightStatus = value;
                    NotifyPropertyChanged("LightStatus");
                }
            }
        }

        private string _roomPressure;
        public string RoomPressure
        {
            get
            {
                return _roomPressure;
            }

            set
            {
                _roomPressure = value;
                NotifyPropertyChanged("RoomPressure");
            }
        }

        private async void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            { 
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() =>
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }
    }

    public class ConferenceRoomDataPointList : ObservableCollection<ConferenceRoomDataPoint>
    {
        public ConferenceRoomDataPointList() : base()
        {
            Add(new ConferenceRoomDataPoint("device1", "Unknown", "0", "0", "Gray", "0", "0", "0"));
            Add(new ConferenceRoomDataPoint("device2", "Unknown", "0", "0", "Gray", "0", "0", "0"));
        }
    }

}
