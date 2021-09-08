// Copyright 2016-2018 Esteve Fernandez <esteve@apache.org>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#ifndef RCLDOTNET_H
#define RCLDOTNET_H

#include "rcldotnet_macros.h"

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_init();

RCLDOTNET_EXPORT
const char * RCLDOTNET_CDECL native_rcl_get_rmw_identifier();

RCLDOTNET_EXPORT
bool RCLDOTNET_CDECL native_rcl_ok();

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_create_node_handle(void **, const char *, const char *);

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_destroy_node_handle(void *node_handle);

RCLDOTNET_EXPORT
void * RCLDOTNET_CDECL native_rcl_get_zero_initialized_wait_set();

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_wait_set_init(
    void *,
    long numberOfSubscriptions,
    long numberOfGuardConditions,
    long numberOfTimers,
    long numberOfClients,
    long numberOfServices,
    long numberOfEvents);

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_wait_set_clear(void *);

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_wait_set_add_subscription(void *, void *);

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_wait_set_add_service(void *wait_set_handle, void *service_handle);

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_wait_set_add_client(void *wait_set_handle, void *client_handle);

RCLDOTNET_EXPORT
void RCLDOTNET_CDECL native_rcl_destroy_wait_set(void *);

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_wait_set(void *, long);

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_take(void *, void *);

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_wait(void *, int64_t);

RCLDOTNET_EXPORT
void * RCLDOTNET_CDECL native_rcl_create_request_header_handle(void);

RCLDOTNET_EXPORT
void RCLDOTNET_CDECL native_rcl_destroy_request_header_handle(void *request_header_handle);

RCLDOTNET_EXPORT
int64_t RCLDOTNET_CDECL native_rcl_request_header_get_sequence_number(void *request_header_handle);

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_take_request(void *service_handle, void *request_header_handle, void *request_handle);

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_send_response(void *service_handle, void *request_header_handle, void *resopnse_handle);

RCLDOTNET_EXPORT
int32_t RCLDOTNET_CDECL native_rcl_take_response(void *client_handle, void *request_header_handle, void *response_handle);

#endif // RCLDOTNET_H
