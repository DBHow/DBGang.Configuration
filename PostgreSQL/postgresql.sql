CREATE TABLE public.app_configuration
(
    key character varying(1024) NOT NULL,
    value text NOT NULL,
    CONSTRAINT app_configuration_pkey PRIMARY KEY (key)
)


-- If you want to use "reloadOnChange" feature, create the following objects
-- A function to send notification
CREATE FUNCTION public."NotifyConfigurationChange"()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
DECLARE 
  data JSON; notification JSON;
BEGIN	

  IF (TG_OP = 'INSERT')     THEN
	 data = row_to_json(NEW);
  ELSIF (TG_OP = 'UPDATE')  THEN
	 data = row_to_json(NEW);
  ELSIF (TG_OP = 'DELETE')  THEN
	 data = row_to_json(OLD);
  END IF;
  
  notification = json_build_object(
            'table',TG_TABLE_NAME,
            'action', TG_OP,
            'data', data);  
			
   PERFORM pg_notify('configchange', notification::TEXT);
   
  RETURN NEW;
END
$BODY$;

-- A trigger to catch changes on app_configuration
CREATE TRIGGER "OnConfigurationChange"
    AFTER INSERT OR DELETE OR UPDATE 
    ON public.app_configuration
    FOR EACH ROW
    EXECUTE PROCEDURE public."NotifyConfigurationChange"();

