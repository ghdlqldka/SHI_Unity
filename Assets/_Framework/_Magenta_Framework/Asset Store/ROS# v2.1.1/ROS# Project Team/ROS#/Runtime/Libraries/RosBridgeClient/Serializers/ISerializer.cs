﻿/*
© Siemens AG, 2020
Author: Berkay Alp Cakal (berkay_alp.cakal.ct@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

- Added GetAll method to DeserializedObject class to return all properties as a string.
    © Siemens AG 2025, Mehmet Emre Cakal, emre.cakal@siemens.com/m.emrecakal@gmail.com
*/

namespace RosSharp.RosBridgeClient
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T obj);
        DeserializedObject Deserialize(byte[] rawData);
        T Deserialize<T>(string JsonString);
    } 

    public abstract class DeserializedObject
    {
        public abstract string GetProperty(string property);
        public abstract string GetAll();
    }
}