﻿using OnlyR.Core.Models;

namespace OnlyR.Model
{
    /// <inheritdoc />
    /// <summary>
    /// Used to configure all mappings between 'data' objects. AutoMapper is a general-purpose data mapper
    /// that we use to map models between layers
    /// </summary>

    public class ObjectMappingProfile : AutoMapper.Profile
    {
        public ObjectMappingProfile()
        {
            // CreateMap<source, dest>();

            CreateMap<RecordingDeviceInfo, RecordingDeviceItem>()
                .ForMember(dest => dest.DeviceId, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceName, opts => opts.MapFrom(src => src.Name));
        }
    }
}
