﻿using System.Collections.Generic;
using System.Xml.Serialization;
using DS4Windows;
using DS4WinWPF.DS4Control.Profiles.Legacy.Migrations;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;

namespace DS4WinWPF.DS4Control.Profiles.Legacy
{
    [XmlRoot(ElementName = "Slot")]
    public class Slot
    {
        [XmlElement(ElementName = "DeviceType")]
        public OutContType DeviceType { get; set; } = OutContType.X360;

        [XmlAttribute(AttributeName = "idx")] 
        public int Idx { get; set; }
    }

    [XmlRoot(ElementName = "OutputSlots")]
    public class OutputSlots : XmlSerializable<OutputSlots>
    {
        [XmlElement(ElementName = "Slot")] 
        public List<Slot> Slot { get; set; } = new();

        [XmlAttribute(AttributeName = "app_version")]
        public string AppVersion { get; set; }

        public override IExtendedXmlSerializer GetSerializer()
        {
            return new ConfigurationContainer()
                .UseOptimizedNamespaces()
                .EnableImplicitTyping(typeof(OutputSlots), typeof(Slot))
                .Type<Slot>().EnableReferences(c => c.Idx)
                .Type<OutputSlots>().AddMigration(new OutputSlotsMigration())
                .EnableMemberExceptionHandling()
                .Create();
        }
    }
}