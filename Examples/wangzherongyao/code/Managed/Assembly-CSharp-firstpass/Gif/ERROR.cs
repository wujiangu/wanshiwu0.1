﻿namespace Gif
{
    using System;

    public enum ERROR
    {
        OK,
        FOUND_TRAILER,
        FAILED_OPEN_FILE,
        NO_SUPPORT_VERSION,
        UNKNOWN_CONTENT_ID,
        UNKNOWN_EXTENSION_ID,
        FORMAT_ERROR
    }
}

