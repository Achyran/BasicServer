using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Packages
{

    public static class PackageFactory
    {

        //To Do: Create a Code Generato that takes care of Encoding and Encoding functions, for Classes that inheret from Package

        public enum Keys
        {
            Welcome = 1,
            WelcomeReceved = 2,
            Heartbeat = 3,
            Message = 4
        }

        public enum Protocoll
        {
            tcp = 1,
            udp = 2
        }



        #region Encoding

        public static byte[] Encode(Package package)
        {
            if (package == null) throw new ArgumentNullException();

            switch (package.key)
            {
                case Keys.Welcome:
                    return Encode((Welcome)package);
                case Keys.WelcomeReceved:
                    return Encode((WelcomeReceved)package);
                case Keys.Heartbeat:
                    return Encode((Heartbeat)package);
                case Keys.Message: 
                    return Encode((Message)package);

                default:
                    return new byte[0];
            }
        }

        private static byte[] Encode(Welcome package)
        {
            List<byte> payload = new List<byte>();

            Write((int)package.key, payload);
            Write(package._id, payload);
            Write(package._message, payload);


            return AddLength(payload);
        }

        private static byte[] Encode(Heartbeat package)
        {
            List<byte> payload = new List<byte>();

            Write((int)package.key, payload);
            Write(package._id, payload);
            return AddLength(payload);
        }

        private static byte[] Encode(WelcomeReceved package)
        {
            List<byte> payload = new List<byte>();

            Write((int)package.key, payload);
            return AddLength(payload);
        }

        private static byte[] Encode(Message package)
        {
            List<byte> payload = new List<byte>();

            Write((int)package.key, payload);
            Write(package.targetUserIds, payload);
            Write(package.originId, payload);
            Write(package.content, payload);
            
            return AddLength(payload);

        }

        private static byte[] AddLength(List<byte> payload)
        {
            List<byte> output = new List<byte>();
            output.AddRange(BitConverter.GetBytes(payload.Count));
            output.AddRange(payload);

            return output.ToArray();
        }

        #endregion
        #region Decoding

        public static Package Decode(List<byte> data)
        {
            if(data == null || data.Count == 0) 
                throw new Exception("No Data Given");

            Keys key = (Keys) GetKey(data);

            switch (key)
            {
                case Keys.Welcome:
                    return DecodeWelcome(data);
                case Keys.WelcomeReceved:
                    return DecodeWelcomeReceved(data);
                case Keys.Heartbeat:
                    return DecodeHeartbeat(data);
                case Keys.Message:
                    return DecodeMessage(data);


                default:
                    throw new Exception($"Could not Create Package: Unknown Key {key}");
            }
        }


        public static Package? TryDecode(List<byte> data)
        {
            try
            {
                return Decode(data.ToList());
            }
            catch (Exception e)
            {

                Console.WriteLine($"Decoding Package Failed: {e.Message}");
                return null;
            }

        }

        private static Welcome DecodeWelcome(List<byte> data)
        {
            int _id;
            string _message;

            int readPoss = 4;
            readPoss += PackageFactory.ReadInt(readPoss, data, out _id);
            readPoss += PackageFactory.ReadString(readPoss, data, out _message);

            return new Welcome(_id, _message);
        }
        private static WelcomeReceved DecodeWelcomeReceved(List<byte> data)
        {
            return new WelcomeReceved();
        }
        public static Message DecodeMessage(List<byte> data)
        {
            int[] targetUsers;
            int origin;
            string content;

            int readPos = 4;

            
            readPos += ReadIntArray(readPos, data, out targetUsers);
            readPos += ReadInt(readPos, data, out origin);
            ReadString(readPos, data, out content);

            return new Message(targetUsers.ToArray(),origin, content);
        }

        private static Heartbeat DecodeHeartbeat(List<byte> data)
        {
            int _id;

            int readPoss = 4;
            readPoss = PackageFactory.ReadInt(readPoss, data, out _id);

            return new Heartbeat(_id);
        }

        #endregion

        #region Reading
        private static int GetKey(List<byte> data)
        {
            if (data.Count >= 4)
            {
                return BitConverter.ToInt32(data.ToArray(),0);
            }
            else
            {
                throw new Exception("Could not Find Key!!");
            }
        }

        /// <summary>Reading the Int on the possition. Returning The Position, and the int as and out.</summary>
        public static int ReadInt(int pos,List<byte> data, out int output)
        {
            if (pos + 4  <= data.Count )
            {
                int _value = BitConverter.ToInt32(data.ToArray(), pos);
                output = _value;
                return 4;
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }
        /// <summary>Reading the String on the possition. Returning The Position, and the string as and out.</summary>
        public static int ReadString(int pos, List<byte> data, out string output)
        {
            try
            {
                int _lenght;
                pos += ReadInt(pos,data,out _lenght); // Get the length of the string
                output = Encoding.ASCII.GetString(data.ToArray(), pos, _lenght); // Convert the bytes to a string
                return pos + _lenght;
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }

        public static int ReadIntArray(int pos, List<byte> data, out int[] output)
        {
            try
            {
                List<int> list = new List<int>();
                int _lenght;
                int readBytes = 0;
                readBytes += ReadInt(pos, data, out _lenght); // Get the length of the string
                for (int i = 0; i < _lenght; i++)
                {
                    int numb;
                    readBytes += ReadInt(pos + readBytes, data, out numb);
                    list.Add(numb);
                }
                output = list.ToArray();
                return readBytes;
            }
            catch
            {
                throw new Exception("Could not read value of type 'int[]'!");
            }
        }

        #endregion
        #region Writing

        /// <summary>Writing int to the end of the byte list.</summary>
        public static void Write(int _value, List<byte> bytelist)
        {
            bytelist.AddRange(BitConverter.GetBytes(_value));
        }

        /// <summary>Writing the string length and the string to the end of the byte list.</summary>
        public static void Write(string _value, List<byte> bytelist)
        {
            Write(_value.Length, bytelist); // Add the length of the string to the packet
            bytelist.AddRange(Encoding.ASCII.GetBytes(_value)); // Add the string itself
        }

        public static void Write(int[] _value, List<byte> bytelist)
        {
            Write(_value.Length,bytelist);

            for (int i = 0; i < _value.Length; i++)
            {
                Write(_value[i],bytelist);
            }
        }
        public static void Write(List<int> _value, List<byte> bytelist)
        {
            Write(_value.ToArray(),bytelist);
        }

        #endregion

        /*
        
        public int ReadInt(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                int _value = BitConverter.ToInt32(readableBuffer, readPos); // Convert the bytes to an int
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 4; // Increase readPos by 4
                }
                return _value; // Return the int
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }


         *  public byte[] InsertKey(int _value, List<byte> _buffer)
        {
             _buffer.InsertRange(0, BitConverter.GetBytes(_value)); // Insert the int at the start of the buffer
        }

        /// <summary>Gets the packet's content in array form.</summary>
        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        /// <summary>Gets the length of the packet's content.</summary>
        public int Length()
        {
            return buffer.Count; // Return the length of buffer
        }

        /// <summary>Gets the length of the unread data contained in the packet.</summary>
        public int UnreadLength()
        {
            return Length() - readPos; // Return the remaining length (unread)
        }

        /// <summary>Resets the packet instance to allow it to be reused.</summary>
        /// <param name="_shouldReset">Whether or not to reset the packet.</param>
        public void Reset(bool _shouldReset = true)
        {
            if (_shouldReset)
            {
                buffer.Clear(); // Clear buffer
                readableBuffer = null;
                readPos = 0; // Reset readPos
            }
            else
            {
                readPos -= 4; // "Unread" the last read int
            }
        }
        #endregion

        #region Write Data
        /// <summary>Adds a byte to the packet.</summary>
        /// <param name="_value">The byte to add.</param>
        public void Write(byte _value)
        {
            buffer.Add(_value);
        }
        /// <summary>Adds an array of bytes to the packet.</summary>
        /// <param name="_value">The byte array to add.</param>
        public void Write(byte[] _value)
        {
            buffer.AddRange(_value);
        }
        /// <summary>Adds a short to the packet.</summary>
        /// <param name="_value">The short to add.</param>
        public void Write(short _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds an int to the packet.</summary>
        /// <param name="_value">The int to add.</param>
        public void Write(int _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds a long to the packet.</summary>
        /// <param name="_value">The long to add.</param>
        public void Write(long _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds a float to the packet.</summary>
        /// <param name="_value">The float to add.</param>
        public void Write(float _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds a bool to the packet.</summary>
        /// <param name="_value">The bool to add.</param>
        public void Write(bool _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds a string to the packet.</summary>
        /// <param name="_value">The string to add.</param>
        public void Write(string _value)
        {
            Write(_value.Length); // Add the length of the string to the packet
            buffer.AddRange(Encoding.ASCII.GetBytes(_value)); // Add the string itself
        }
        #endregion

        #region Read Data
        /// <summary>Reads a byte from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public byte ReadByte(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                byte _value = readableBuffer[readPos]; // Get the byte at readPos' position
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 1; // Increase readPos by 1
                }
                return _value; // Return the byte
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        /// <summary>Reads an array of bytes from the packet.</summary>
        /// <param name="_length">The length of the byte array.</param>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public byte[] ReadBytes(int _length, bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                byte[] _value = buffer.GetRange(readPos, _length).ToArray(); // Get the bytes at readPos' position with a range of _length
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += _length; // Increase readPos by _length
                }
                return _value; // Return the bytes
            }
            else
            {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        /// <summary>Reads a short from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public short ReadShort(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                short _value = BitConverter.ToInt16(readableBuffer, readPos); // Convert the bytes to a short
                if (_moveReadPos)
                {
                    // If _moveReadPos is true and there are unread bytes
                    readPos += 2; // Increase readPos by 2
                }
                return _value; // Return the short
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        /// <summary>Reads an int from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public int ReadInt(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                int _value = BitConverter.ToInt32(readableBuffer, readPos); // Convert the bytes to an int
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 4; // Increase readPos by 4
                }
                return _value; // Return the int
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        /// <summary>Reads a long from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public long ReadLong(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                long _value = BitConverter.ToInt64(readableBuffer, readPos); // Convert the bytes to a long
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 8; // Increase readPos by 8
                }
                return _value; // Return the long
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        /// <summary>Reads a float from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public float ReadFloat(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                float _value = BitConverter.ToSingle(readableBuffer, readPos); // Convert the bytes to a float
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 4; // Increase readPos by 4
                }
                return _value; // Return the float
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        /// <summary>Reads a bool from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public bool ReadBool(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                bool _value = BitConverter.ToBoolean(readableBuffer, readPos); // Convert the bytes to a bool
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 1; // Increase readPos by 1
                }
                return _value; // Return the bool
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        /// <summary>Reads a string from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public string ReadString(bool _moveReadPos = true)
        {
            try
            {
                int _length = ReadInt(); // Get the length of the string
                string _value = Encoding.ASCII.GetString(readableBuffer, readPos, _length); // Convert the bytes to a string
                if (_moveReadPos && _value.Length > 0)
                {
                    // If _moveReadPos is true string is not empty
                    readPos += _length; // Increase readPos by the length of the string
                }
                return _value; // Return the string
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }
        #endregion

        private bool disposed = false;

        protected virtual void Dispose(bool _disposing)
        {
            if (!disposed)
            {
                if (_disposing)
                {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }

                disposed = true;
            }
        }
        */
    }
}